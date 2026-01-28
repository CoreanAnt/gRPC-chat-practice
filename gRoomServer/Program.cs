using gRoom.gRPC.Services;

var builder = WebApplication.CreateBuilder(args);

// 서버에 통신기능 추가
builder.Services.AddGrpc();

//객체생성
var app = builder.Build();

// 예제는 서비스 1개만 사용.
app.MapGrpcService<GroomService>();

//그냥 설명문, 굳이 작성함(필요없는 코드).
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
