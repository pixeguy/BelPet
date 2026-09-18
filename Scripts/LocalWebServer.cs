using Godot;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public partial class LocalWebServer : Node
{
    private HttpListener listener;
    public event Action serverStarted;
    public event Action<string> codeReceived;

    public async Task StartServer()
    {
        listener = new HttpListener();
        listener.Prefixes.Add("http://127.0.0.1:8888/");
        listener.Start();

        serverStarted?.Invoke();

        GD.Print("Local server running at http://127.0.0.1:8888/");

        while (listener.IsListening)
        {
            try
            {
                HttpListenerContext context =
                    await listener.GetContextAsync();

                await HandleRequest(context);
            }
            catch (Exception e)
            {
                GD.PrintErr(e.Message);
            }
        }
    }

    private async Task HandleRequest(HttpListenerContext context)
    {
        string requestPath = context.Request.Url.AbsolutePath;

        if (requestPath == "/callback")
        {
            WaitForSpotifyCode(context);
            return;
        }

        if (requestPath == "/spotify.html")
        {
            await ServeSpotifyHtml(context);
            return;
        }
    }

    private void WaitForSpotifyCode(HttpListenerContext context)
    {
        string code = context.Request.QueryString["code"];

        if (code != null)
        {
            codeReceived?.Invoke(code);
        }

        string message = "Spotify connected! You can close this window.";
        byte[] data = Encoding.UTF8.GetBytes(message);

        context.Response.StatusCode = 200;
        context.Response.ContentType = "text/plain";
        context.Response.ContentLength64 = data.Length;

        context.Response.OutputStream.Write(data);
        context.Response.Close();
    }

    private async Task ServeSpotifyHtml(HttpListenerContext context)
    {
        string path = ProjectSettings.GlobalizePath("res://Web/spotify.html");
        string html = await System.IO.File.ReadAllTextAsync(path);

        byte[] data = Encoding.UTF8.GetBytes(html);

        context.Response.StatusCode = 200;
        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.ContentLength64 = data.Length;

        await context.Response.OutputStream.WriteAsync(data);
        context.Response.Close();
    }

    public override void _ExitTree()
    {
        if (listener != null && listener.IsListening)
        {
            listener.Stop();
            listener.Close();
        }
    }
}
