from flask import Flask, request, jsonify
import threading

app = Flask(__name__)

# 전역 변수로 인증 정보를 저장
auth_data = {}

@app.route("/")
def home():
    """OAuth 인증 후 code 값 처리"""
    global auth_data
    code = request.args.get("code")
    state = request.args.get("state")
    
    if code and state:
        auth_data = {"code": code, "state": state}  # code 값을 저장
        return jsonify(auth_data)  # JSON 응답
    elif auth_data:  # 이전에 받은 code가 있다면 반환
        return jsonify(auth_data)
    else:
        return jsonify({"error": "Code not received"}), 400  # 오류 처리


@app.route("/shutdown", methods=["POST"])
def shutdown_request():
    """Flask 서버 종료 요청 엔드포인트"""
    func = request.environ.get("werkzeug.server.shutdown")
    if func is None:
        return jsonify({"error": "Not running the Werkzeug Server"}), 500
    func()
    print("🛑 Flask server stopped.")
    return jsonify({"message": "Server shutting down..."}), 200
    
    
def run_flask():
    """Flask 서버 실행"""
    app.run(port=8080)
    
if __name__ == "__main__":
    run_flask()
