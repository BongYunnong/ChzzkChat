using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// # 채널 정보 조회
/// </summary>
public class ChzzkChannelComponent : ChzzkComponentBase
{
    [System.Serializable]
    public class ChannelResult
    {
        public string channelId;
        public string channelName;
        public string channelImageUrl;
        public string followerCount;
    }

    private List<string> channelIds = new List<string>(); 
    private string pendingChannelId;
    
    [SerializeField] private Button addChannelButton;
    [SerializeField] private Button removeChannelButton;
    [SerializeField] private TMP_InputField channelIdInputField;

    public override EAPICategory GetAPICategory()
    {
        return EAPICategory.Channel;
    }

    private void Start()
    {
        addChannelButton.onClick.AddListener(AddChannelId);
        removeChannelButton.onClick.AddListener(RemoveChannelId);
        channelIdInputField.onValueChanged.AddListener(HandleChannelIdChanged);
        
        pendingChannelId = channelIdInputField.text;
    }

    private void HandleChannelIdChanged(string value)
    {
        pendingChannelId = value;
    }
    
    public override void DoAction(string action, string parameter)
    {
        base.DoAction(action, parameter);

        if (action == "GET")
        {
            StartCoroutine(GetChannelInfo());
        }
    }

    public void AddChannelId()
    {
        int foundedIndex = channelIds.FindIndex(x=>x.Equals(pendingChannelId));
        if (foundedIndex >= 0)
        {
            return;
        }

        if (string.IsNullOrEmpty(pendingChannelId))
        {
            Debug.LogError("[AddChannelId] pendingChannelId is Null");
            return;
        }
        channelIds.Add(pendingChannelId);
        if (channelIds.Count > 20)
        {
            Debug.LogError("[AddChannelId] Channel IDs count is more than 20.");
        }
    }
    public void RemoveChannelId()
    {
        int foundedIndex = channelIds.FindIndex(x=>x.Equals(pendingChannelId));
        if (foundedIndex >= 0)
        {
            channelIds.RemoveAt(foundedIndex);
        }
    }

    IEnumerator GetChannelInfo()
    {
        string url = $"{ChzzkController.BaseURL}/open/v1/channels";
        string queryString = string.Join("&", channelIds.Select(item => $"channelIds={UnityWebRequest.EscapeURL(item)}"));
        string finalUrl = $"{url}?{queryString}";
        Debug.Log($"[Channel] Requset URL : {finalUrl}");
        using (UnityWebRequest request = UnityWebRequest.Get($"{finalUrl}"))
        {
            request.SetRequestHeader("Client-Id", cachedChzzkController.ClientId);
            request.SetRequestHeader("Client-Secret", cachedChzzkController.ClientSecret);
            request.SetRequestHeader("Content-Type", ChzzkController.ContentType);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[Channel] Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("[Channel] Error: " + request.error);
            }
        }
    }
}
