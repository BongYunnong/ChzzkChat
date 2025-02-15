import threading
import time
import requests
import socketio
import json

from flask_server import run_flask
from oauth import request_oauth
from config import CLIENT_ID, CLIENT_SECRET, REDIRECT_URI, STATE
from config import URL, URL_AccessToken, URL_CreateSession, URL_SessionList
from config import URL_SubscribeChat, URL_SubscribeDonation

# Step 1: Flask 서버를 백그라운드에서 실행
flask_thread = threading.Thread(target=run_flask, daemon=True)
flask_thread.start()
time.sleep(2)  # 서버가 실행될 시간을 줌

# Step 2: OAuth 인증 요청
code, state = request_oauth()

session_key = [None]  

if code and state:
    print("🛑 Sending shutdown request to Flask server...")
    try:
        requests.post("http://localhost:8080/shutdown")
    except requests.exceptions.RequestException as e:
        print(f"⚠️ Error shutting down Flask server: {e}")
    
    
    response = requests.post(URL_AccessToken,
        json={
            "grantType":"authorization_code",
            "clientId":CLIENT_ID,
            "clientSecret":CLIENT_SECRET,
            "code":code,"state":state
            })

    if response.status_code == 200:
        data = response.json()  # JSON 응답 파싱
        content = data.get("content", {})  # "content" 키 아래의 값 가져오기

        access_token = content.get("accessToken")
        refresh_token = content.get("refreshToken")
        token_type = content.get("tokenType")
        expires_in = content.get("expiresIn")
        scope = content.get("scope")

        print(f"✅ Access Token: {access_token}")
        print(f"🔄 Refresh Token: {refresh_token}")
        print(f"🔖 Token Type: {token_type}")
        print(f"⏳ Expires In: {expires_in} seconds")
        print(f"📜 Scope: {scope}")
    else:
        print(f"❌ Error: {response.status_code} - {response.text}")
    print(f"🔹 Shutdown Response: {response.status_code} - {response.text}")
    
    
    
    headers = {
        "Authorization": F"Bearer {access_token}",
        "Content-Type": "application/json"
    }
    
    sessionResponse = requests.get(URL_CreateSession, headers=headers)
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
        sessionListResponse = requests.get(URL_SessionList, headers=headers, params=params)
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
else:
    print("❌ Authentication failed.")
    exit(1)  # 인증 실패 시 프로그램 종료
