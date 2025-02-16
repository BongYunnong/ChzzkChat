using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using UnityEngine.Networking;
using Quobject.SocketIoClientDotNet.Client;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine.Serialization;



/// <summary>
/// # 세션 생성(클라이언트)
/// # 세션 생성(유저)
/// # 세션 목록 조회(클라이언트)
/// # 세션 목록 조회(유저)
/// # 이벤트 구독(채팅)
/// # 이벤트 구독 취소(채팅)
/// # 이벤트 구독(후원)
/// # 이벤트 구독 취소(후원)
/// </summary>
public class ChzzkSessionComponent : ChzzkComponentBase
{
    [Serializable]
    public class CreateSessionResult
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
    
    private Socket socket;
    
    private string sessionURL = "";
    private string sessionKey = null;

    [SerializeField] private bool subscribeChatOnConnect = false;
    [SerializeField] private bool subscribeDonationOnConnect = false;
    
    private const string URL_SubscribeChat = "https://openapi.chzzk.naver.com/open/v1/sessions/events/subscribe/chat";
    private const string URL_UnSubscribeChat = "https://openapi.chzzk.naver.com/open/v1/sessions/events/unsubscribe/chat";
    private const string URL_SubscribeDonation = "https://openapi.chzzk.naver.com/open/v1/sessions/events/subscribe/donation";
    private const string URL_UnSubscribeDonation = "https://openapi.chzzk.naver.com/open/v1/sessions/events/unsubscribe/donation";
    
    public override void DoAction(string action, string parameter)
    {
        base.DoAction(action, parameter);

        if (action == "CONNECT")
        {
            StartCoroutine(CreateUserSession());
        }
        else if (action == "SUBSCRIBE")
        {
            string type = GetParamValue("type");
            if (type == "CHAT")
            {
                StartCoroutine(SubscribeChat());
            }
            else if (type == "DONATION")
            {
                StartCoroutine(SubscribeDonation());
            }
        }
        else if (action == "UNSUBSCRIBE")
        {
            string type = GetParamValue("type");
            if (type == "CHAT")
            {
                StartCoroutine(UnsubscribeChat());
            }
            else if (type == "DONATION")
            {
                StartCoroutine(UnsubscribeDonation());
            }
        }
        else if (action == "SESSIONLIST")
        {
            StartCoroutine(GetClientSessionList());
        }
    }

    IEnumerator CreateUserSession()
    {
        // 세션 생성(유저)
        string url = $"{ChzzkController.BaseURL}/open/v1/sessions/auth";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"{cachedChzzkController.TokenType} {cachedChzzkController.AccessToken}");
            request.SetRequestHeader("Content-Type", ChzzkController.ContentType);
            request.SetRequestHeader("Client-Id", cachedChzzkController.ClientId);
            request.SetRequestHeader("Client-Secret", cachedChzzkController.ClientSecret);

            yield return request.SendWebRequest();
            CreateSessionResult accessTokenResult = JsonUtility.FromJson<CreateSessionResult>(request.downloadHandler.text);
            if (request.result == UnityWebRequest.Result.Success)
            {
                sessionURL = accessTokenResult.content.url;
                Debug.Log("[CreateUserSession] Response: " + sessionURL);
                yield return ConnectSocket();
            }
            else
            {
                Debug.LogError("[CreateUserSession] Error: " + request.error);
            }
        }
    }
    

    IEnumerator ConnectSocket()
    {
        sessionURL = sessionURL.Replace("https:", "wss:");
        Debug.LogWarning("[ConnectSocket] sessionURL : " + sessionURL);

        Uri uri = new Uri(sessionURL);
        string serverUrl = uri.Scheme + "://" + uri.Host + ":" + uri.Port;

        var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
        string authToken = queryParams["auth"];
        
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
            Debug.LogWarning("[ConnectSocket]  Successfully connected to Socket.IO server!");
        });

        socket.On(Socket.EVENT_DISCONNECT, () =>
        {
            Debug.LogWarning("[ConnectSocket] Disconnected from server.");
        });
        
        socket.On("SYSTEM", (ev) => {
            Debug.LogWarning($"[ConnectSocket] Received SYSTEM message: {ev}");
            JObject json = JObject.Parse(ev.ToString());
            string parsedSessionkey = json["data"]?["sessionKey"]?.ToString();
            if (string.IsNullOrEmpty(parsedSessionkey) == false)
            {
                sessionKey = parsedSessionkey;
                Debug.Log($"[SYSTEM] Extracted sessionKey: {sessionKey}");
            }
        });
        
        socket.On("CHAT", (ev) => {
            Debug.Log($"[ConnectSocket]  Received CHAT message: {ev}");
        });

        socket.On("DONATION", (ev) => {
            Debug.Log($"[ConnectSocket] Received DONTATION message: {ev}");
        });

        
        socket.Connect();
        
        // System 메시지를 통해 sessionKey를 받을 때까지 대기
        while (string.IsNullOrEmpty(sessionKey))
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        // 채팅
        if (subscribeChatOnConnect)
        {
            StartCoroutine(SubscribeChat());
        }

        // 도네이션
        if (subscribeDonationOnConnect)
        {
            StartCoroutine(SubscribeDonation());
        }
    }

    IEnumerator SubscribeChat()
    {
        string subscribeChatURL = $"{URL_SubscribeChat}?sessionKey={sessionKey}";
        using (UnityWebRequest subscribeChatRequest = new UnityWebRequest(subscribeChatURL, "POST"))
        {
            subscribeChatRequest.SetRequestHeader("Authorization", $"{cachedChzzkController.TokenType} {cachedChzzkController.AccessToken}");
            subscribeChatRequest.SetRequestHeader("Content-Type", ChzzkController.ContentType);
            yield return subscribeChatRequest.SendWebRequest();
            if (subscribeChatRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[CHAT] subscribeChat Succeeded");
            }
            else
            {
                Debug.LogWarning("[CHAT] subscribeChat Failed");
            }
        }
    }

    IEnumerator SubscribeDonation()
    {
        string subscribeDonationURL = $"{URL_SubscribeDonation}?sessionKey={sessionKey}";
        using (UnityWebRequest subscribeDonationRequest = new UnityWebRequest(subscribeDonationURL, "POST"))
        {
            subscribeDonationRequest.SetRequestHeader("Authorization", $"{cachedChzzkController.TokenType} {cachedChzzkController.AccessToken}");
            subscribeDonationRequest.SetRequestHeader("Content-Type", ChzzkController.ContentType);
            yield return subscribeDonationRequest.SendWebRequest();
            if (subscribeDonationRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[DONATION] subscribeDonation Succeeded");
            }
            else
            {
                Debug.LogWarning("[DONATION] subscribeDonation Failed");
            }
        }
    }

    IEnumerator UnsubscribeChat()
    {
        string url = $"{URL_UnSubscribeChat}?sessionKey={sessionKey}";
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.SetRequestHeader("Authorization", $"{cachedChzzkController.TokenType} {cachedChzzkController.AccessToken}");
            request.SetRequestHeader("Content-Type", ChzzkController.ContentType);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[CHAT] UnsubscribeChat Succeeded");
            }
            else
            {
                Debug.LogWarning("[CHAT] UnsubscribeChat Failed");
            }
        }
    }
    
    IEnumerator UnsubscribeDonation()
    {
        string url = $"{URL_UnSubscribeDonation}?sessionKey={sessionKey}";
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.SetRequestHeader("Authorization", $"{cachedChzzkController.TokenType} {cachedChzzkController.AccessToken}");
            request.SetRequestHeader("Content-Type", ChzzkController.ContentType);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[DONATION] UnsubscribeDonation Succeeded");
            }
            else
            {
                Debug.LogWarning("[DONATION] UnsubscribeDonation Failed");
            }
        }
    }
    
    IEnumerator GetClientSessionList()
    {
        string url = $"{ChzzkController.BaseURL}/open/v1/sessions";
        using (UnityWebRequest request = UnityWebRequest.Get($"{url}"))
        {
            request.SetRequestHeader("Authorization", $"{cachedChzzkController.TokenType} {cachedChzzkController.AccessToken}");
            request.SetRequestHeader("Content-Type", ChzzkController.ContentType);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[SessionList] Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("[SessionList] Error: " + request.error);
            }
        }
    }
    
    public override EAPICategory GetAPICategory()
    {
        return EAPICategory.Session;
    }
}