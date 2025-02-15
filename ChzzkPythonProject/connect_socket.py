import socketio

# Socket.IO 클라이언트 생성
sio = socketio.Client()

# 연결 성공 시 실행될 이벤트 핸들러
@sio.event
def connect():
    print("✅ Connected to the WebSocket server!")

# 연결 종료 시 실행될 이벤트 핸들러
@sio.event
def disconnect():
    print("🛑 Disconnected from the WebSocket server.")

# 서버에서 메시지를 받을 때 실행될 핸들러 (이벤트명을 맞춰줘야 함)
@sio.on("message")
def on_message(data):
    print(f"📩 Received message: {data}")

# WebSocket 서버 URL (예제에서 받은 `sessionURL` 사용)
sessionURL = "https://ssio08.nchat.naver.com:443?auth=TOKEN"

try:
    # 서버에 연결
    sio.connect(sessionURL, transports=["websocket"])

    # 연결된 상태 유지
    sio.wait()

except Exception as e:
    print(f"❌ Connection failed: {e}")
