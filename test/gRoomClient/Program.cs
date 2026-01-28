using Grpc.Net.Client;
using gRoom.gRPC.Messages;

using var channel = GrpcChannel.ForAddress("http://localhost:5181");
var client=new Groom.GroomClient(channel);
Console.Write("Enter room name: ");
var roomname=Console.ReadLine();
var resp=client.RegisterToRoom(new RoomRegistrationRequest{RoomName=roomname});
Console.WriteLine($"Room Id: {resp.RoomId}");

Console.Read();