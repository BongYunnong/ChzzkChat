using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Net;
using Quobject.SocketIoClientDotNet.Client;
using Newtonsoft.Json.Linq;

[Serializable]
public class AuthorizationCodeResult
{
    public int code;
    public string message;
    public Content content;

    [Serializable]
    public class Content
    {
        public string code;
        public string state;
    }
}


[Serializable]
public class SessionResult
{
    public int code;
    public string message;
    public Content content;

    [Serializable]
    public class Content
    {
        public string url;
    }
}


[Serializable]
public class AccessTokenResult
{
    public int code;
    public string message;
    public Content content;

    [Serializable]
    public class Content
    {
        public string refreshToken;
        public string accessToken;
        public string tokenType;
        public string expiresIn;
    }
}
[Serializable]
public class RefreshTokenResult
{
    public int code;
    public string message;
    public Content content;

    [Serializable]
    public class Content
    {
        public string accessToken;
        public string refreshToken;
        public string tokenType;
        public string expiresIn;
        public string scope;
    }
}

public class ChzzkChat : MonoBehaviour
{
    [System.Serializable]
    public class TokenRequest
    {
        public string grantType;
        public string clientId;
        public string clientSecret;
        public string code;
        public string state;
    }
    [System.Serializable]
    public class RefreshTokenRequest
    {
        public string grantType;
        public string refreshToken;
        public string clientId;
        public string clientSecret;
    }

    [System.Serializable]
    public class AccessTokenRequest
    {
        public string clientId;
        public string redirectUri;
        public string state;
    }


    [System.Serializable]
    public class ChannelRequest
    {
        public string grantType;
        public string clientId;
        public string clientSecret;
        public string code;
        public string state;
    }

    private string baseUrl = "https://openapi.chzzk.naver.com";


    [SerializeField] private string clientId = "f827034c-632c-42cf-9889-9000674573d0";
    [SerializeField] private string clientSecret = "j6ZIhUJkDhdfNy0HaERDEbjhxNrFdirUIbW_B8QcSmE";
    private string contentType = "application/json";
    private string redirectUri = "http://localhost:8080";
    private string sessionURL = "";

    [SerializeField] private string authorization = "";
    private string refreshToken = "";
    private string accessToken = "";
    private string tokenType = "";
    private string expiresln = "";

    private string code = "";
    private string state = "";

    private Quobject.SocketIoClientDotNet.Client.Socket socket;
    
    string sessionKey = null;
    
    private string loginUrl = "";

    void Start()
    {
        StartCoroutine(GetUserSession());
    }

    IEnumerator GetAccessToken()
    {
        yield return new WaitForSeconds(1);
        loginUrl = $"https://chzzk.naver.com/account-interlock?clientId={clientId}&redirectUri={redirectUri}&state={state}";
        using (UnityWebRequest request = UnityWebRequest.Get(loginUrl))
        {
            request.redirectLimit = 5;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Final URL: " + request.GetResponseHeader("Location"));
                Debug.Log("Final URL: " + request.url);
                Debug.Log("Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }

    IEnumerator GetUserSession()
    {
        //yield return StartCoroutine(GetAuthorizationCode());
        yield return StartCoroutine(PostAccessToken());

        string url = $"{baseUrl}/open/v1/sessions/auth";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"{tokenType} {accessToken}");
            request.SetRequestHeader("Content-Type", contentType);
            request.SetRequestHeader("Client-Id", clientId);
            request.SetRequestHeader("Client-Secret", clientSecret);

            yield return request.SendWebRequest();
            SessionResult accessTokenResult = JsonUtility.FromJson<SessionResult>(request.downloadHandler.text);
            if (request.result == UnityWebRequest.Result.Success)
            {
                sessionURL = accessTokenResult.content.url;
                Debug.Log("Response: " + sessionURL);
                yield return ConnectSocket();
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }
    IEnumerator PostAccessToken()
    {
        string url = $"{baseUrl}/auth/v1/token";

        string json = JsonUtility.ToJson(new TokenRequest
        {
            grantType = "authorization_code",
            clientId = clientId,
            clientSecret = clientSecret,
            code = authorization,
            state = "State"
        });

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AccessTokenResult accessTokenResult = JsonUtility.FromJson<AccessTokenResult>(request.downloadHandler.text);

                Debug.Log("refreshToken: " + accessTokenResult.content.refreshToken);
                Debug.Log("accessToken: " + accessTokenResult.content.accessToken);
                Debug.Log("tokenType: " + accessTokenResult.content.tokenType);
                Debug.Log("expiresln: " + accessTokenResult.content.expiresIn);
                refreshToken = accessTokenResult.content.refreshToken;
                accessToken = accessTokenResult.content.accessToken;
                tokenType = accessTokenResult.content.tokenType;
                expiresln = accessTokenResult.content.expiresIn;

                string refreshJson = JsonUtility.ToJson(new RefreshTokenRequest
                {
                    grantType = "refresh_token",
                    refreshToken = refreshToken,
                    clientId = clientId,
                    clientSecret = clientSecret
                });
                byte[] refreshBodyRaw = Encoding.UTF8.GetBytes(refreshJson);
                using (UnityWebRequest refreshRequest = new UnityWebRequest(url, "POST"))
                {
                    refreshRequest.uploadHandler = new UploadHandlerRaw(refreshBodyRaw);
                    refreshRequest.downloadHandler = new DownloadHandlerBuffer();
                    refreshRequest.SetRequestHeader("Content-Type", "application/json");


                    yield return refreshRequest.SendWebRequest();
                    if (refreshRequest.result == UnityWebRequest.Result.Success)
                    {
                        RefreshTokenResult refreshTokenResult = JsonUtility.FromJson<RefreshTokenResult>(refreshRequest.downloadHandler.text);
                        Debug.Log("refreshToken: " + refreshTokenResult.content.refreshToken);
                        Debug.Log("accessToken: " + refreshTokenResult.content.accessToken);
                        Debug.Log("tokenType: " + refreshTokenResult.content.tokenType);
                        Debug.Log("expiresln: " + refreshTokenResult.content.expiresIn);
                        Debug.Log("scope: " + refreshTokenResult.content.scope);
                        refreshToken = refreshTokenResult.content.refreshToken;
                        accessToken = refreshTokenResult.content.accessToken;
                        tokenType = refreshTokenResult.content.tokenType;
                        expiresln = refreshTokenResult.content.expiresIn;
                        
                    }
                }
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }


    IEnumerator GetAuthorizationCode()
    {
        string url = $"https://chzzk.naver.com/account-interlock";

        string json = JsonUtility.ToJson(new AccessTokenRequest
        {
            clientId = clientId,
            redirectUri = "http://localhost:8080",
            state = "zxclDasdfA25"
        });

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();
            Debug.Log("response: " + request.downloadHandler.text);

            AuthorizationCodeResult authorizationCodeResult = JsonUtility.FromJson<AuthorizationCodeResult>(request.downloadHandler.text);
            if (request.result == UnityWebRequest.Result.Success)
            {
                code = authorizationCodeResult.content.code;
                state = authorizationCodeResult.content.state;
                Debug.Log("code: " + code);
                Debug.Log("state: " + state);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }


    IEnumerator ConnectSocket()
    {
        sessionURL = sessionURL.Replace("https:", "wss:");
        Debug.LogWarning("sessionURL : " + sessionURL);

        Uri uri = new Uri(sessionURL);
        string serverUrl = uri.Scheme + "://" + uri.Host + ":" + uri.Port;

        var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
        string authToken = queryParams["auth"];

        Debug.Log($"🔹 Parsed Server URL: {serverUrl}");
        Debug.Log($"🔹 Parsed Auth Token: {authToken}");
        
        var options = new IO.Options
        {
            Reconnection = false,
            ForceNew = true,
            Timeout = 3000,
            Transports = ImmutableList.Create("websocket"),
            Query = new Dictionary<string, string>
            {
                { "auth", authToken },
            },
        };

        Manager manager = new Manager(new Uri(serverUrl), options);
        socket = manager.Socket("/");
        
        socket.On(Socket.EVENT_CONNECT, () =>
        {
            Debug.Log("✅ Successfully connected to Socket.IO server!");
        });

        socket.On(Socket.EVENT_DISCONNECT, () =>
        {
            Debug.Log("❌ Disconnected from server.");
        });
        
        socket.On("CHAT", (ev) => {
            Debug.Log($"🔹 Received CHAT message: {ev}");
        });

        socket.On("DONATION", (ev) => {
            Debug.Log($"🔹 Received DONTATION message: {ev}");
        });

        
        socket.On("SYSTEM", (ev) => {
            Debug.Log($"🔹 Received SYSTEM message: {ev}");
            JObject json = JObject.Parse(ev.ToString());
            sessionKey = json["data"]?["sessionKey"]?.ToString();

            Console.WriteLine($"🔹 Extracted sessionKey: {sessionKey}");
        });
        
        socket.Connect();

        while (string.IsNullOrEmpty(sessionKey))
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        // 채팅
        string URL_SubscribeChat = "https://openapi.chzzk.naver.com/open/v1/sessions/events/subscribe/chat";
        string subscribeChatURL = $"{URL_SubscribeChat}?sessionKey={sessionKey}";
        using (UnityWebRequest subscribeChatRequest = new UnityWebRequest(subscribeChatURL, "POST"))
        {
            subscribeChatRequest.SetRequestHeader("Authorization", $"{tokenType} {accessToken}");
            subscribeChatRequest.SetRequestHeader("Content-Type", contentType);
            yield return subscribeChatRequest.SendWebRequest();
            if (subscribeChatRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("subscribeChat Succeeded");
            }
            else
            {
                Debug.LogWarning("subscribeChat Failed");
            }
        }

        // 도네이션
        /*
        string URL_SubscribeDonation = "https://openapi.chzzk.naver.com/open/v1/sessions/events/subscribe/donation";
        string subscribeDonationURL = $"{URL_SubscribeDonation}?sessionKey={sessionKey}";
        using (UnityWebRequest subscribeDonationRequest = new UnityWebRequest(subscribeDonationURL, "POST"))
        {
            subscribeDonationRequest.SetRequestHeader("Authorization", $"{tokenType} {accessToken}");
            subscribeDonationRequest.SetRequestHeader("Content-Type", contentType);
            yield return subscribeDonationRequest.SendWebRequest();
            if (subscribeDonationRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("subscribeDonation Succeeded");
            }
            else
            {
                Debug.LogWarning("subscribeDonation Failed");
            }
        }
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
        }
        */
    }
}