# gRPC Real-time Chat Application

gRPC를 활용한 실시간 채팅 및 번역 서비스 프로젝트입니다.

## 기술 스택
- **Server**: C# (.NET 9.0)
- **Client**: C# (.NET 9.0)
- **Bot**: Node.js

## 프로젝트 설명
- `gRoomServer`: 채팅 서버
- `FullRoomClient`: 사용자 클라이언트
- `gRoomAdmin`: 관리자 클라이언트
- `newsbot`: 뉴스 봇 클라이언트

## 순서
1. 먼저 gRoomServer를 킨다
2. gRoomAdmin를 킨다.
3. FullRoomClient를 켜서 방 이름이 같게하고 채팅을 한다.(여러개 가능)
4. newsbot을 켜서 클라이언트에 가상 뉴스를 뿌려주는지 확인한다.