using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;


public class ChzzkUserComponent : ChzzkComponentBase
{
    [Serializable]
    public class UseResult
    {
        public int code;
        public string message;
        public Content content;

        [Serializable]
        public class Content
        {
            public string channelId;
            public string channelName;
            public string nickName;
        }
    }
    
    public override EAPICategory GetAPICategory()
    {
        return EAPICategory.User;
    }
    
    public override void DoAction(string action, string parameter)
    {
        base.DoAction(action, parameter);

        if (action == "GET")
        {
            StartCoroutine(GetUserInfo());
        }
    }

    IEnumerator GetUserInfo()
    {
        string url = $"{ChzzkController.BaseURL}/open/v1/users/me";
        using (UnityWebRequest request = UnityWebRequest.Get($"{url}"))
        {
            request.SetRequestHeader("Authorization", $"{cachedChzzkController.TokenType} {cachedChzzkController.AccessToken}");
            request.SetRequestHeader("Content-Type", ChzzkController.ContentType);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[User] Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("[User] Error: " + request.error);
            }
        }
    }
}
