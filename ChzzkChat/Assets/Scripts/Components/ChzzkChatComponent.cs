using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// # 채팅 메시지 전송
/// # 채팅 공지 등록
/// # 채팅 설정 조회
/// # 채팅 설정 변경
/// </summary>
public class ChzzkChatComponent : ChzzkComponentBase
{
    [System.Serializable]
    public class SendChatRequest
    {
        public string message = null; // 최대 100자로 제한
    }
    [System.Serializable]
    public class NoticeRequest
    {
        public string message = null; // 최대 100자로 제한
        public string messageId = null; // 기존 메시지로 공지사항 등록 시 사용하는 전송된 메시지 ID
    }
    
    [System.Serializable]
    public class SettingRequest
    {
        public string chatAvailableCondition = null;    // NONE, REAL_NAME
        public string chatAvailableGroup = null;    // ALL, FOLLOWER, MANAGER, SUBSCRIBER
        public int minFollowerMinute = 0; // 0, 5, 10, 30, 60, 1440, 10080, 43200 값만 허용
        public bool allowSubscriberInFollowerMode = false; // FOLLOWER 모드 설정된 경우 구독자는 최소 팔로잉 기간 조건 대상에서 제외 허용 할지 여부
    }
    
    [SerializeField] private InputField sendChatInputField;
    [SerializeField] private InputField noticeInputField;
    [SerializeField] private InputField noticeMessageIdInputField;
    
    [SerializeField] private InputField chatAvailableConditionInputField;
    [SerializeField] private InputField chatAvailableGroupInputField;
    [SerializeField] private InputField minFollowerMinuteInputField;

    [SerializeField] private Toggle allowSubscriberInFollowerModeToggle;

    private string message = null;
    
    private string noticeMesage = null;
    private string noticeMesageId = null;
    
    private string chatAvailableCondition = null;
    private string chatAvailableGroup = null;
    private int minFollowerMinute = 0;
    private bool allowSubscriberInFollowerMode = false;
    
    private void Start()
    {
        sendChatInputField.onValueChanged.AddListener(HandleChatMessageChanged);
        HandleChatMessageChanged(sendChatInputField.text);
        
        noticeInputField.onValueChanged.AddListener(HandleNoticeMessageChanged);
        HandleNoticeMessageChanged(noticeInputField.text);
        noticeMessageIdInputField.onValueChanged.AddListener(HandleNoticeMessageIdChanged);
        HandleNoticeMessageIdChanged(noticeMessageIdInputField.text);
        
        chatAvailableConditionInputField.onValueChanged.AddListener(HandleChatAvailableConditionChanged);
        HandleChatAvailableConditionChanged(chatAvailableConditionInputField.text);
        chatAvailableGroupInputField.onValueChanged.AddListener(HandlechatAvailableGroupChanged);
        HandlechatAvailableGroupChanged(chatAvailableGroupInputField.text);
        minFollowerMinuteInputField.onValueChanged.AddListener(HandleMinFollowerMinuteChanged);
        HandleMinFollowerMinuteChanged(minFollowerMinuteInputField.text);
        
        allowSubscriberInFollowerModeToggle.onValueChanged.AddListener(HandleMinFollowerMinuteChanged);
        HandleMinFollowerMinuteChanged(allowSubscriberInFollowerMode);
    }
    
    private void HandleChatMessageChanged(string value)
    {
        message = value;
    }
    private void HandleNoticeMessageChanged(string value)
    {
        noticeMesage = value;
    }
    private void HandleNoticeMessageIdChanged(string value)
    {
        noticeMesageId = value;
    }
    
    private void HandleChatAvailableConditionChanged(string value)
    {
        chatAvailableCondition = value;
    }
    private void HandlechatAvailableGroupChanged(string value)
    {
        chatAvailableGroup = value;
    }
    private void HandleMinFollowerMinuteChanged(string value)
    {
        minFollowerMinute = int.Parse(value);
    }
    private void HandleMinFollowerMinuteChanged(bool value)
    {
        allowSubscriberInFollowerMode = value;
    }
    
    public override EAPICategory GetAPICategory()
    {
        return EAPICategory.Chat;
    }

    public override void DoAction(string action, string parameter)
    {
        base.DoAction(action, parameter);

        if (action == "CHAT")
        {
            StartCoroutine(SendChatMessage());
        }
        else if (action == "NOTICE")
        {
            StartCoroutine(SetNotice());
        }
        else if (action == "TONOTICE")
        {
            StartCoroutine(SetToNotice());
        }
        else if (action == "GETSETTING")
        {
            StartCoroutine(GetSettingInfo());
        }
        else if (action == "POSTSETTING")
        {
            StartCoroutine(PostSettingInfo());
        }
    }

    
    IEnumerator SendChatMessage()
    {
        string url = $"{ChzzkController.BaseURL}/open/v1/chats/send";
        string sendChatRequest = JsonUtility.ToJson(new SendChatRequest()
        {
            message = this.message,
        });
        
        Debug.Log($"[SendChatMessage] send chat {sendChatRequest}");
        
        byte[] requestBodyRaw = Encoding.UTF8.GetBytes(sendChatRequest);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(requestBodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            request.SetRequestHeader("Authorization", $"{cachedChzzkController.TokenType} {cachedChzzkController.AccessToken}");
            request.SetRequestHeader("Content-Type", ChzzkController.ContentType);
            
            yield return request.SendWebRequest();
            Debug.Log($"[SendChatMessage] Send Chat {request.downloadHandler.text}");
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[SendChatMessage] Send Chat Succeeded");
            }
            else
            {
                Debug.LogWarning("[SendChatMessage] Send Chat Failed");
            }
        }
    }
    
    IEnumerator SetNotice()
    {
        string url = $"{ChzzkController.BaseURL}/open/v1/chats/notice";
        string noticeRequest = JsonUtility.ToJson(new NoticeRequest
        {
            message = this.noticeMesage,
        });
        
        Debug.Log($"[SetNotice] send chat {noticeRequest}");
        
        byte[] requestBodyRaw = Encoding.UTF8.GetBytes(noticeRequest);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(requestBodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            request.SetRequestHeader("Authorization", $"{cachedChzzkController.TokenType} {cachedChzzkController.AccessToken}");
            request.SetRequestHeader("Content-Type", ChzzkController.ContentType);
            
            yield return request.SendWebRequest();
            Debug.Log($"[SetNotice] Set Notice {request.downloadHandler.text}");
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[SetNotice] Set Notice Succeeded");
            }
            else
            {
                Debug.LogWarning("[SetNotice] Set Notice Failed");
            }
        }
    }
    
    IEnumerator SetToNotice()
    {
        string url = $"{ChzzkController.BaseURL}/open/v1/chats/notice";
        string noticeRequest = JsonUtility.ToJson(new NoticeRequest
        {
            messageId = this.noticeMesageId,
        });
        
        Debug.Log($"[SetNotice] send chat {noticeRequest}");
        
        byte[] requestBodyRaw = Encoding.UTF8.GetBytes(noticeRequest);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(requestBodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            request.SetRequestHeader("Authorization", $"{cachedChzzkController.TokenType} {cachedChzzkController.AccessToken}");
            request.SetRequestHeader("Content-Type", ChzzkController.ContentType);
            
            yield return request.SendWebRequest();
            Debug.Log($"[SetNotice] Set Notice {request.downloadHandler.text}");
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[SetNotice] Set Notice Succeeded");
            }
            else
            {
                Debug.LogWarning("[SetNotice] Set Notice Failed");
            }
        }
    }
    IEnumerator GetSettingInfo()
    {
        string url = $"{ChzzkController.BaseURL}/open/v1/chats/settings";
        using (UnityWebRequest request = UnityWebRequest.Get($"{url}"))
        {
            request.SetRequestHeader("Authorization", $"{cachedChzzkController.TokenType} {cachedChzzkController.AccessToken}");
            request.SetRequestHeader("Content-Type", ChzzkController.ContentType);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[Setting] Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("[Setting] Error: " + request.error);
            }
        }
    }
    
    IEnumerator PostSettingInfo()
    {
        string url = $"{ChzzkController.BaseURL}/open/v1/chats/settings";
        
        string settingRequest = JsonUtility.ToJson(new SettingRequest
        {
            chatAvailableCondition = this.chatAvailableCondition,
            chatAvailableGroup = this.chatAvailableGroup,
            minFollowerMinute = this.minFollowerMinute,
            allowSubscriberInFollowerMode = this.allowSubscriberInFollowerMode,
        });
        
        Debug.Log($"[Set Setting] settingRequestJson {settingRequest}");
        
        byte[] requestBodyRaw = Encoding.UTF8.GetBytes(settingRequest);
        using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(requestBodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            request.SetRequestHeader("Authorization", $"{cachedChzzkController.TokenType} {cachedChzzkController.AccessToken}");
            request.SetRequestHeader("Content-Type", ChzzkController.ContentType);
            
            yield return request.SendWebRequest();
            Debug.Log($"[Set Setting] Set Setting {request.downloadHandler.text}");
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[Set Setting] Set Setting Succeeded");
            }
            else
            {
                Debug.LogWarning("[Set Setting] Set Setting Failed");
            }
        }
    }
}
