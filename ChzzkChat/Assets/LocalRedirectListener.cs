using System;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class LocalRedirectListener : MonoBehaviour
{
    private HttpListener listener;
    private string redirectUrl = "http://localhost:8080/";

    async void Start()
    {
        listener = new HttpListener();
        listener.Prefixes.Add(redirectUrl);
        listener.Start();
        Debug.Log("Listening on: " + redirectUrl);

        await ListenForRedirect();
    }

    private async Task ListenForRedirect()
    {
        while (listener.IsListening)
        {
            var context = await listener.GetContextAsync();
            string redirectedUrl = context.Request.Url.ToString();
            Debug.Log($"Redirected URL: {redirectedUrl}");

            // ���� ���� (�������� ǥ��)
            string responseString = "<html><body><h1>Login Successful!</h1></body></html>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            context.Response.ContentLength64 = buffer.Length;
            var output = context.Response.OutputStream;
            await output.WriteAsync(buffer, 0, buffer.Length);
            output.Close();

            listener.Stop(); // �α��� �Ϸ� �� ���� ����
        }
    }

    private void OnApplicationQuit()
    {
        if (listener != null && listener.IsListening)
        {
            listener.Stop();
        }
    }
}
