import requests
import webbrowser
import time
from config import CLIENT_ID, CLIENT_SECRET, REDIRECT_URI, STATE, URL

def get_auth_url():
    """OAuth 인증 URL 생성"""
    return f"{URL}?clientId={CLIENT_ID}&redirectUri={REDIRECT_URI}&state={STATE}"
    
def request_oauth():
    """브라우저에서 OAuth 인증 수행"""
    auth_url = get_auth_url()
    print("🌐 Opening browser for authentication...")
    webbrowser.open(auth_url)

    # Flask 서버에서 code 값을 받을 때까지 대기
    max_retries = 10
    retry_delay = 2  # 2초 간격

    for i in range(max_retries):
        print(f"🔄 Trying to fetch authentication code... Attempt {i+1}/{max_retries}")
        response = requests.get(REDIRECT_URI)

        if response.status_code == 200:
            data = response.json()
            if "code" in data:
                code = data["code"]
                state = data["state"]
                print(f"✅ Authentication successful!")
                print(f"🔑 Code: {code}")
                print(f"📌 State: {state}")
                return code, state  # 인증 완료 후 반환
        else:
            print("❌ Code not received yet, retrying...")

        time.sleep(retry_delay)

    print("❌ Authentication failed.")
    return None, None