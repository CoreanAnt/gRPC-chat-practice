using gRoom.gRPC.Messages;
using Google.Protobuf.WellKnownTypes;

namespace gRoom.gRPC.Utils;

public class UsersQueues  {
    private static List<UserQueue> _queues;
    private static Queue<ReceivedMessage> _adminQueue;

    static UsersQueues()  {
        _queues = new List<UserQueue>();
        _adminQueue = new Queue<ReceivedMessage>();
    }

    public static void CreateUserQueue(String room, String user)  {
        _queues.Add(new UserQueue(room, user));
    }    

    public static void AddMessageToRoom(ReceivedMessage msg, string room)  {
        // 룸번호 있는 유저만 메시지 추가
        foreach (var queue in _queues.Where(q=>q.Room==room))  {
            queue.AddMessageToQueue(msg);
        }
        _adminQueue.Enqueue(msg);
    }

    public static void AddNewsToAllUsers(ReceivedMessage msg)
    {
        foreach (var queue in _queues) {
            queue.AddMessageToQueue(msg);
        }
        _adminQueue.Enqueue(msg);
    }

    public static ReceivedMessage GetMessageForUser(string user)  {
        //나는 직관성 때문에 개인적으로 foreach문이 더 좋은데......(FirstOrDefault 특정조건 첫번째 데이터 검색)
        var userQueue = _queues.FirstOrDefault(q => q.User == user);
        if (userQueue != null && userQueue.GetMessagesCount()>0)  {
            return userQueue.GetNextMessage();
        }
        return null;
    }  

    public static int GetAdminQueueMessageCount()  {
        return _adminQueue.Count;
    }  

    public static ReceivedMessage GetNextAdminMessage()  {
        return _adminQueue.Dequeue();
    }
}

class UserQueue  {
    private Queue<ReceivedMessage> queue  { get; }
    public string Room { get; }
    public string User { get; }

    public UserQueue(string room, string user)  {
        Room = room;
        User = user;
        this.queue = new Queue<ReceivedMessage>();
    }

    public void AddMessageToQueue(ReceivedMessage msg)  {
        this.queue.Enqueue(msg);
    }

    public ReceivedMessage GetNextMessage()  {
        return this.queue.Dequeue();
    }  

    public int GetMessagesCount()  {
        return this.queue.Count;
    } 
}