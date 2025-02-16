using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

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
public class RevokeTokenRequest
{
    public string clientId;
    public string clientSecret;
    public string token;
    public string tokenTypeHint;
}

public enum EAPICategory
{
    Default,
    Session,
    User,
    Channel,
    Category,
    Live,
    Chat,
    Drops
}

public class ChzzkController : MonoBehaviour
{
    public static readonly string BaseURL = "https://openapi.chzzk.naver.com";
    public static readonly string AuthURL = "https://chzzk.naver.com/account-interlock";
    public static readonly string ContentType = "application/json";
    
    [SerializeField] private string clientId = "";
    public string ClientId { get { return clientId; } }
    [SerializeField] private string clientSecret = "";
    public string ClientSecret { get { return clientSecret; } }
    [SerializeField] private LocalRedirectListener redirectListener;

    private string redirectUri = "http://localhost:8080";
    private string state = "";
    
    private string accessCode = "";
    private bool accessed = false;
    
    private string refreshToken = "";
    private string accessToken = "";
    public string AccessToken { get { return accessToken; } }
    private string tokenType = "";
    public string TokenType { get { return tokenType; } }
    private string expiresln = "";

    private Dictionary<EAPICategory, ChzzkComponentBase> chzzkComponents = new Dictionary<EAPICategory, ChzzkComponentBase>();

    private void Awake()
    {
        // 컨트롤러 게임오브젝트에 붙어있는 치지직 컴포넌트들을 연결
        ChzzkComponentBase[] components = GetComponentsInChildren<ChzzkComponentBase>();
        for (int i = 0; i < components.Length; i++)
        {
            ChzzkComponentBase component = components[i];
            component.InitializeComponent(this);
            chzzkComponents.Add(component.GetAPICategory(), component);
        }
    }

    void Start()
    {
        SetupController();
    }

    private void SetupController()
    {
        state = GetRandomState(5);
        // 웹 브라우저에서 OAuth 인증을 필요로하기에 그냥 Get으로 받을수 없음.
        // 대신 HttpListener를 활용해서 Redirection에 오는 이벤트를 구독
        string accessCodeRequestURL = $"{AuthURL}?clientId={clientId}&redirectUri={redirectUri}&state={state}";
        Application.OpenURL(accessCodeRequestURL);
    }

    private string GetRandomState(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Range(0, s.Length)]).ToArray());
    }
    
    void Update()
    {
        if (accessed || string.IsNullOrEmpty(redirectListener.code))
        {
            return;
        }
        accessed = true;
        this.accessCode = redirectListener.code;
        StartCoroutine(PostAccessToken());
    }
    
    IEnumerator PostAccessToken()
    {
        // accessToken의 만료기간은 1일, refreshToken의 만료기간은 30일
        string accessTokenURL = $"{BaseURL}/auth/v1/token";
        
        // Get AccessToken
        string accessTokenJson = JsonUtility.ToJson(new TokenRequest
        {
            grantType = "authorization_code",
            clientId = clientId,
            clientSecret = clientSecret,
            code = this.accessCode,
            state = this.state
        });
        byte[] bodyRaw = Encoding.UTF8.GetBytes(accessTokenJson);
        using (UnityWebRequest request = new UnityWebRequest(accessTokenURL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", ContentType);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AccessTokenResult accessTokenResult = JsonUtility.FromJson<AccessTokenResult>(request.downloadHandler.text);

                Debug.Log("=====Get Access Token=====");
                Debug.Log("refreshToken: " + accessTokenResult.content.refreshToken);
                Debug.Log("accessToken: " + accessTokenResult.content.accessToken);
                Debug.Log("tokenType: " + accessTokenResult.content.tokenType);
                Debug.Log("expiresln: " + accessTokenResult.content.expiresIn);
                refreshToken = accessTokenResult.content.refreshToken;
                accessToken = accessTokenResult.content.accessToken;
                tokenType = accessTokenResult.content.tokenType;
                expiresln = accessTokenResult.content.expiresIn;
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
        
        // Refresh AccessToken
        string refreshTokenJson = JsonUtility.ToJson(new RefreshTokenRequest
        {
            grantType = "refresh_token",
            refreshToken = refreshToken,
            clientId = clientId,
            clientSecret = clientSecret
        });
        byte[] refreshBodyRaw = Encoding.UTF8.GetBytes(refreshTokenJson);
        using (UnityWebRequest refreshRequest = new UnityWebRequest(accessTokenURL, "POST"))
        {
            refreshRequest.uploadHandler = new UploadHandlerRaw(refreshBodyRaw);
            refreshRequest.downloadHandler = new DownloadHandlerBuffer();
            refreshRequest.SetRequestHeader("Content-Type", ContentType);

            yield return refreshRequest.SendWebRequest();

            if (refreshRequest.result == UnityWebRequest.Result.Success)
            {
                RefreshTokenResult refreshTokenResult = JsonUtility.FromJson<RefreshTokenResult>(refreshRequest.downloadHandler.text);
                
                Debug.Log("=====Refresh Access Token=====");
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
    
    IEnumerator RevokeAccessToken(bool useRefreshToken = false)
    {
        string revokeTokenURL = $"{BaseURL}/auth/v1/token/revoke";
        
        string revokeTokenJson = JsonUtility.ToJson(new RevokeTokenRequest()
        {
            clientId = clientId,
            clientSecret = clientSecret,
            token = useRefreshToken ? refreshToken : accessToken,
            tokenTypeHint = useRefreshToken ? "refresh_token" : "access_token",
        });
        byte[] bodyRaw = Encoding.UTF8.GetBytes(revokeTokenJson);
        using (UnityWebRequest request = new UnityWebRequest(revokeTokenURL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", ContentType);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("=====Revoke Access Token Succeeded=====");
            }
            else
            {
                Debug.LogWarning("=====Revoke Access Token Failed=====");
            }
        }
    }

    public void SendAction(string actionString)
    {
        string[] tokens = actionString.Split('/');
        EAPICategory category = Enum.Parse<EAPICategory>(tokens[0]);
        if (category == EAPICategory.Default)
        {
            if (tokens[1] == "CONNECT")
            {
                accessed = false;
                redirectListener.StartListen();
                SetupController();
            }
            else if (tokens[1] == "DISCONNECT")
            {
                StartCoroutine(RevokeAccessToken());
            }
        }
        else
        {
            string action = tokens.Length > 1 ? tokens[1] : null;
            string param = tokens.Length > 2 ? tokens[2] : null;
            chzzkComponents[category].DoAction(action, param);
        }
    }
}
