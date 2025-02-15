using System;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class LocalRedirectListener : MonoBehaviour
{
    private HttpListener listener;
    private string redirectUrl = "http://localhost:8080/";
    private bool isRunning = false;

    public UnityAction<string> onRedirected;

    public string code = null;
    
    private void Start()
    {
        listener = new HttpListener();
        listener.Prefixes.Add(redirectUrl);
        listener.Start();
        Debug.Log("Listening on: " + redirectUrl);

        Invoke("StartListen", 1.0f);
    }

    private void StartListen()
    {
        // 요청 비동기 처리
        Task.Run(() => ListenForRedirect());
    }

    private async Task ListenForRedirect()
    {
        while (listener.IsListening)
        {
            var context = await listener.GetContextAsync();
            var request = context.Request;
            string redirectedUrl = context.Request.Url.ToString();

            if (request.Url != null)
            {
                Debug.Log($"Received Request: {request.Url.AbsoluteUri}");
                    
                // URL에서 token 값 추출
                var queriedCode = request.QueryString["code"];
                if (!string.IsNullOrEmpty(queriedCode))
                {
                    Debug.Log($"Extracted Code: {queriedCode}");
                    code = queriedCode;
                }
            }
            listener.Stop();
        }
    }

    private void OnApplicationQuit()
    {
        isRunning = false;
        if (listener != null && listener.IsListening)
        {
            listener.Stop();
            listener.Close();
        }
    }
}
