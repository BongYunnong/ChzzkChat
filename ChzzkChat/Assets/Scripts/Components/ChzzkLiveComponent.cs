using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// # 라이브 목록 조회
/// # 방송 스트림키 조회
/// # 방송 설정 조회
/// # 방송 설정 변경
/// </summary>
public class ChzzkLiveComponent : ChzzkComponentBase
{
    [System.Serializable]
    public class SettingRequest
    {
        public string defaultLiveTitle = null;
        public string categoryType = null;
        public string categoryId = null;
        public string[] tags = null;
    }
    
    [SerializeField] private Slider querySizeSlider;
    [SerializeField] private TMP_Text querySizeText;
    [SerializeField] private TMP_Text nextPageText;

    [SerializeField] private InputField liveTitleInputField;
    [SerializeField] private InputField categoryTypeInputField;
    [SerializeField] private InputField categoryIdInputField;
    [SerializeField] private InputField tagsInputField;
    
    private int size = 20;
    private string next = null;
    
    private string liveTitle = null;
    private string categoryType = null;
    private string categoryId = null;
    private string tags = null;
    
    private void Start()
    {
        querySizeSlider.onValueChanged.AddListener(HandleQuerySizeChanged);
        querySizeSlider.value = size;
        
        
        liveTitleInputField.onValueChanged.AddListener(HandleLiveTitleChanged);
        liveTitle = liveTitleInputField.text;
        categoryTypeInputField.onValueChanged.AddListener(HandleCategoryTypeChanged);
        categoryType = categoryTypeInputField.text;
        categoryIdInputField.onValueChanged.AddListener(HandleCategoryIdChanged);
        categoryId = categoryIdInputField.text;
        tagsInputField.onValueChanged.AddListener(HandleTagsChanged);
        tags = tagsInputField.text;
    }

    private void HandleLiveTitleChanged(string value)
    {
        liveTitle = value;
    }

    private void HandleCategoryTypeChanged(string value)
    {
        categoryType = value;
    }

    private void HandleCategoryIdChanged(string value)
    {
        categoryId = value;
    }

    private void HandleTagsChanged(string value)
    {
        tags = value;
    }

    private void HandleQuerySizeChanged(float size)
    {
        this.size = Mathf.FloorToInt(size);
        querySizeText.SetText(size.ToString());
    }

    public override EAPICategory GetAPICategory()
    {
        return EAPICategory.Live;
    }
    
    public override void DoAction(string action, string parameter)
    {
        base.DoAction(action, parameter);

        if (action == "GET")
        {
            StartCoroutine(GetLiveInfo());
        }
        else if (action == "STREAMKEY")
        {
            StartCoroutine(GetStreamKeyInfo());
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
    
    IEnumerator GetLiveInfo()
    {
        string url = $"{ChzzkController.BaseURL}/open/v1/lives?size={size}";
        if (string.IsNullOrEmpty(next) == false)
        {
            url += $"&next={next}";
        }
        Debug.Log($"[Live] Requset URL : {url}");
        using (UnityWebRequest request = UnityWebRequest.Get($"{url}"))
        {
            request.SetRequestHeader("Client-Id", cachedChzzkController.ClientId);
            request.SetRequestHeader("Client-Secret", cachedChzzkController.ClientSecret);
            request.SetRequestHeader("Content-Type", ChzzkController.ContentType);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[Live] Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("[Live] Error: " + request.error);
            }
        }
    }
    
    IEnumerator GetStreamKeyInfo()
    {
        string url = $"{ChzzkController.BaseURL}/open/v1/streams/key";
        using (UnityWebRequest request = UnityWebRequest.Get($"{url}"))
        {
            request.SetRequestHeader("Authorization", $"{cachedChzzkController.TokenType} {cachedChzzkController.AccessToken}");
            request.SetRequestHeader("Content-Type", ChzzkController.ContentType);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[StreamKey] Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("[StreamKey] Error: " + request.error);
            }
        }
    }
    
    IEnumerator GetSettingInfo()
    {
        string url = $"{ChzzkController.BaseURL}/open/v1/lives/setting";
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
        string url = $"{ChzzkController.BaseURL}/open/v1/lives/setting";
        
        // CategoryType은 CategoryId와 연결되어야 하는 듯
        // CategoryId는 그냥 한글로 되어있는 것이 아니라 "과학/기술"->"sci-tech" 이런 id로 되어있음
        string settingRequest = JsonUtility.ToJson(new SettingRequest
        {
            defaultLiveTitle = liveTitle,
            categoryType = categoryType,
            categoryId = categoryId,
            tags = tags.Split(';'),
        });
        
        Debug.Log($"[Set Setting] settingRequestJson {settingRequest}");
        
        byte[] requestBodyRaw = Encoding.UTF8.GetBytes(settingRequest);
        using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
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
