import threading
import time
import requests
import socketio
import json

from oauth import request_oauth
from config import CLIENT_ID, CLIENT_SECRET, REDIRECT_URI, STATE
from config import URL, URL_AccessToken, URL_CreateSession, URL_SessionList
from config import URL_SubscribeChat, URL_SubscribeDonation
from config import URL_CreateSession_Client, URL_SessionList_Client

session_key = [None]  

    
headers = {
    "Client-Id": CLIENT_ID,
    "Client-Secret": CLIENT_SECRET,
    "Content-Type": "application/json"
}

sessionResponse = requests.get(URL_CreateSession_Client, headers=headers)
if sessionResponse.status_code == 200:
    data = sessionResponse.json()  # JSON 응답 파싱
    content = data.get("content", {})  # "content" 키 아래의 값 가져오기

    session_url = content.get("url")
    print(f"✅ session url: {session_url}")
    
    
    def connect_socket(url):
        sio = socketio.Client()

        @sio.event
        def connect():
            print("✅ Connected to the WebSocket server!")

        @sio.event
        def disconnect():
            print("🛑 Disconnected from the WebSocket server.")

        @sio.on("SYSTEM")
        def on_system_event(data):
            print(f"🔔 SYSTEM Event Received: {data}")
            sessionData = json.loads(data)
            try:
                if sessionData.get("type") == "connected":
                    session_key[0] = sessionData["data"]["sessionKey"]
                    print(f"🔑 Extracted Session Key: {session_key[0]}")
            except KeyError as e:
                print(f"⚠️ KeyError: {e} | Data: {data}")
                
                
        @sio.on("CHAT")
        def on_chat_event(data):
            print(f"🔔 CHAT Event Received: {data}")
            
        @sio.on("DONATION")
        def on_donation_event(data):
            print(f"🔔 DONATION Event Received: {data}")
            
                
        try:
            sio.connect(url, transports=["websocket"])
            sio.wait()
        except Exception as e:
            print(f"❌ WebSocket Connection failed: {e}")

    session_url = session_url.replace("https://", "wss://")
    
    socket_thread = threading.Thread(target=connect_socket, args=(session_url,), daemon=True)
    socket_thread.start()
    
    while session_key[0] is None:
        print(f"✅ waiting Socket Connection")
        time.sleep(1)  # 무한 루프를 돌면서 Socket을 유지
    
    # 채팅
    subscribeURL = f"{URL_SubscribeChat}?sessionKey={session_key[0]}"
    subscribeChatResponse = requests.post(subscribeURL, headers=headers)

    if subscribeChatResponse.status_code == 200:
        print(f"✅ subscribeChat Succeeded: {subscribeChatResponse.text}")
    else:
        print(f"❌ Failed to subscribe chat: {subscribeChatResponse.status_code} - {subscribeChatResponse.text}")
        exit(1)  # WebSocket 세션 생성 실패 시 프로그램 종료


    # 도네이션 
    subscribeURL = f"{URL_SubscribeDonation}?sessionKey={session_key[0]}"
    subscribeDonationResponse = requests.post(subscribeURL, headers=headers)

    if subscribeDonationResponse.status_code == 200:
        print(f"✅ subscribeDonation Succeeded: {subscribeDonationResponse.text}")
    else:
        print(f"❌ Failed to subscribe donation: {subscribeDonationResponse.status_code} - {subscribeDonationResponse.text}")
        exit(1)  # WebSocket 세션 생성 실패 시 프로그램 종료
        
    params = {
        "size":50,
        "page":0
    }
    sessionListResponse = requests.get(URL_SessionList_Client, headers=headers, params=params)
    if sessionListResponse.status_code == 200:
        session_list = sessionListResponse.json().get("data", [])
        print(f"✅ Retrieved {len(session_list)} sessions")
    else:
        print(f"❌ Failed to fetch session list: {response.status_code} - {response.text}")
        exit(1)  # WebSocket 세션 생성 실패 시 프로그램 종료

    while True:
        time.sleep(1)  # 무한 루프를 돌면서 Socket을 유지
    
else:
    print(f"❌ Error: {sessionResponse.status_code} - {sessionResponse.text}")
    exit(1)  # WebSocket 세션 생성 실패 시 프로그램 종료
