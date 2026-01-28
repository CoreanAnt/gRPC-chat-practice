using Grpc.Core;
using gRoom.gRPC.Messages;
using Google.Protobuf.WellKnownTypes;
using gRoom.gRPC.Utils;

namespace gRoom.gRPC.Services;

public class GroomService : Groom.GroomBase
{
    private readonly ILogger<GroomService> _logger;
    public GroomService(ILogger<GroomService> logger)
    {
        _logger = logger;
    }
	//context에 토큰 정보가 같이 포함되어 옴.
    public override async Task<RoomRegistrationResponse> RegisterToRoom(RoomRegistrationRequest request, ServerCallContext context)
    {
        //인위적인 딜레이 FullRoomClient의 데드라인 작동 확인을 위한 코드.
        //await Task.Delay(10000);

        UsersQueues.CreateUserQueue(request.RoomName, request.UserName);
        var resp = new RoomRegistrationResponse(){Joined=true};
        return await Task.FromResult(resp);

        // 응답확인코드
        // _logger.LogInformation("Service called...");
        // var roomNum = Random.Shared.Next(1, 100); //.NET6.0이상
        // _logger.LogInformation($"Room no. {roomNum}");
        // var resp = new RoomRegistrationResponse { RoomId = roomNum };
        // return Task.FromResult(resp);
    }

    public override async Task<NewsStreamStatus> SendNewsFlash(IAsyncStreamReader<NewsFlash> NewsStream, ServerCallContext context)
    {
        while(await NewsStream.MoveNext())
        {
            var news = NewsStream.Current;
            var msg = new ReceivedMessage { Contents = news.NewsItem, User = "NewsBot", MsgTime = Timestamp.FromDateTime(DateTime.UtcNow) };
            UsersQueues.AddNewsToAllUsers(msg);
            //MessagesQueue.AddNewsToQueue(news); 저장해서 하나씩 뿌리지 말고 전체에 공지
            Console.WriteLine($"News flash: {news.NewsItem}");
        }

        return new NewsStreamStatus{Success=true};
    }

    public override async Task StartMonitoring(Empty request, IServerStreamWriter<ReceivedMessage> streamWriter, ServerCallContext context)
    {
        while (true)
        {
            // 테스트
            // await streamWriter.WriteAsync(new ReceivedMessage{
            //     MsgTime=Timestamp.FromDateTime(DateTime.UtcNow), 
            //     User="1", 
            //     Contents="Test msg"});
            if (MessagesQueue.GetMessagesCount() > 0)
            {
                await streamWriter.WriteAsync(MessagesQueue.GetNextMessage());
            }
            if (UsersQueues.GetAdminQueueMessageCount() > 0)
            {
                await streamWriter.WriteAsync(UsersQueues.GetNextAdminMessage());
            }
            await Task.Delay(500);
        }
    }

    public override async Task StartChat(IAsyncStreamReader<ChatMessage> incomingStream, IServerStreamWriter<ChatMessage> outgoingStream, ServerCallContext context)  {
	    // 예제는 이렇게 작성하는데 권유는 if (await incomingStream.MoveNext()) 하고 바로 실행하여야 한다고 한다. 그리고 else로 실패시 대처.
        // C# 8.0이후는 foreach (var data in incomingStream.ReadAllAsync()) 이런식으로 data.Contents로 사용한다더라.
        while (!await incomingStream.MoveNext())  {
            await Task.Delay(100);
        }

        string userName = incomingStream.Current.User;
        string room = incomingStream.Current.Room;
        Console.WriteLine($"User {userName} connected to room {room}");

        //UsersQueues.CreateUserQueue(room, userName);

        // 유저에게 메시지 받기
        var reqTask = Task.Run(async () =>
        {
            while (await incomingStream.MoveNext())
            {
                Console.WriteLine($"Message received: {incomingStream.Current.Contents}");
                UsersQueues.AddMessageToRoom(ConvertToReceivedMessage(incomingStream.Current), incomingStream.Current.Room);
            }
        });

        // Check for messages to send to the user
        var respTask = Task.Run(async () =>
        {
            while (true)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                var userMsg = UsersQueues.GetMessageForUser(userName);
                if (userMsg != null)
                {
                    var userMessage = ConvertToChatMessage(userMsg, room);
                    await outgoingStream.WriteAsync(userMessage);
                }
                // 저장해서 하나씩 뿌리지 말고 전체에 공지
                // if (MessagesQueue.GetMessagesCount() > 0)
                // {
                //     var news = MessagesQueue.GetNextMessage();
                //     var newsMessage = ConvertToChatMessage(news, room);
                //     await outgoingStream.WriteAsync(newsMessage);
                // }

                await Task.Delay(200);
            }
        });

        // await Task.WhenAny(reqTask, respTask); 실제는 이렇게 사용한다던데......
        while (true)  {
            await Task.Delay(10000);
        }
    }

    private ReceivedMessage ConvertToReceivedMessage(ChatMessage chatMsg)  {
	var rcMsg = new ReceivedMessage();
	rcMsg.Contents = chatMsg.Contents;
	rcMsg.MsgTime = chatMsg.MsgTime;
	rcMsg.User = chatMsg.User;

	return rcMsg;
    }

    private ChatMessage ConvertToChatMessage(ReceivedMessage rcMsg, string room)  {
        var chatMsg = new ChatMessage();
        chatMsg.Contents = rcMsg.Contents;
        chatMsg.MsgTime = rcMsg.MsgTime;
        chatMsg.User = rcMsg.User;
        chatMsg.Room = room;

        return chatMsg;
    }
}
