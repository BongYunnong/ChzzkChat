using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ChzzkChatClient : MonoBehaviour
{
    IEnumerator GetClientChannel()
    {
        /*
        string channelId = "08cbcf6f38129e27612712d7207664a2";
        string url = $"https://openapi.chzzk.naver.com/open/v1/channels?channelIds={channelId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Client-Id", clientId);
            request.SetRequestHeader("Client-Secret", clientSecret);
            request.SetRequestHeader("Content-Type", contentType);
            

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
        */
        yield break;
    }
    
    IEnumerator GetClientSession()
    {
        /*
        string url = $"{baseUrl}/open/v1/sessions/auth/client";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Client-Id", clientId);
            request.SetRequestHeader("Client-Secret", clientSecret);
            request.SetRequestHeader("Content-Type", contentType);

            yield return request.SendWebRequest();
            SessionResult accessTokenResult = JsonUtility.FromJson<SessionResult>(request.downloadHandler.text);
            if (request.result == UnityWebRequest.Result.Success)
            {
                sessionURL = accessTokenResult.content.url;
                Debug.Log("Response: " + sessionURL);

                
                yield return GetClientChannel();
                yield return GetClientSessionList();
                Connect().Forget();
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
        */
        yield break;
    }
    

    IEnumerator GetClientSessionList()
    {
        /*
        string url = $"{baseUrl}/open/v1/sessions/client";
        using (UnityWebRequest request = UnityWebRequest.Get($"{url}"))
        {
            request.SetRequestHeader("Client-Id", clientId);
            request.SetRequestHeader("Client-Secret", clientSecret);
            request.SetRequestHeader("Content-Type", contentType);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
        */
        yield break;
    }
}
