using Godot;
using System;
using System.Threading.Tasks;

public partial class SpotifyManager : Node
{
    [Export]
    private LocalWebServer server;
    [Export]
    private SpotifyAuth auth;
    [Export]
    private SpotifyAPI api;
    [Export]
    private Node webView;

    private bool isPlayerActive = false;
    private double songPosition;
    private double songDuration;
    private bool isPaused = true;
    private float volume;

    public double SongProgress => songPosition;
    public double SongDuration => songDuration;
    public bool IsPaused => isPaused;
    public float Volume => volume;

    public event Action spotifyApiReady;
    public event Action spotifyPlayerReady;
    public event Action<string> onSongChanged;
    public event Action<bool> onPlayBackChanged;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        server.codeReceived += OnCodeReceived;
        server.serverStarted += OnServerStarted;
        auth.accessTokenReceived += OnAccessTokenReceived;
        api.OnFetchNewState += OnFetchNewState;
        api.OnRefreshNewState += OnRefreshNewState;

        webView.Connect("ipc_message", Callable.From<string>(OnWebViewMessage));

        _ = server.StartServer();
    }

    private void OnWebViewMessage(string message)
    {
        Json json = new Json();
        Error error = json.Parse(message);
        if (error != Error.Ok)
        {
            GD.PrintErr("Failed to parse WebView message: " + message);
            return;
        }

        Godot.Collections.Dictionary data = (Godot.Collections.Dictionary)json.Data;
        string type = data["type"].AsString();
        switch (type)
        {
            case "player_ready":
                {
                    string deviceId = data["deviceId"].AsString();
                    spotifyPlayerReady?.Invoke();
                    break;
                }
            case "track_changed":
                {
                    string trackId = data["trackId"].AsString();
                    string imageUrl = data["imageUrl"].AsString();
                    api.SetTrackId(trackId);
                    onSongChanged?.Invoke(imageUrl);
                    break;
                }
            case "playback_state":
                {
                    songPosition = data["position"].AsDouble();
                    songDuration = data["duration"].AsDouble();
                    isPaused = data["paused"].AsBool();
                    break;
                }
            case "player_active":
                {
                    isPlayerActive = true;
                    api.StopFetchTimer();
                    break;
                }
            case "player_inactive":
                {
                    isPlayerActive = false;
                    api.StartFetchTimer();
                    break;
                }
            case "volume":
                volume = data["volume"].AsSingle();
                break;
        }
    }

    public async void NextSong()
    {
        if (isPlayerActive)
            webView.Call("eval", "nextTrack();");
        else
        { 
            await api.SkipToNextSong();
            await api.RefreshNow();
        } 
    }

    public async void PrevSong()
    {
        if (isPlayerActive)
            webView.Call("eval", "previousTrack();");
        else
        {
            await api.SkipToPrevSong();
            await api.RefreshNow();
        }
    }

    public void ChangeVolume(float amount)
    {
        volume = Mathf.Clamp(volume + amount, 0f, 1f);
        if (isPlayerActive)
            webView.Call("eval", $"setVolume({volume});");
        else
            api.ScheduleVolumeChange(volume * 100);
    }

    public async void SetSongProgress(float progress)
    {
        double positionMs = songDuration * progress;
        songPosition = positionMs;

        if (isPlayerActive)
        {
            webView.Call("eval", $"seek({(long)positionMs});");
            return;
        }

        await api.SetSongProgress(positionMs);
    }

    public async void PlayOrPauseSong()
    {
        if (isPlayerActive)
        {
            webView.Call("eval", "togglePlay();");
            onPlayBackChanged?.Invoke(isPaused);
            return;
        }

        if (isPaused)
        {
            await api.Play();
            isPaused = false;
        }
        else
        {
            await api.Pause();
            isPaused = true;
        }

        onPlayBackChanged?.Invoke(isPaused);
    }

    private void OnFetchNewState(Godot.Collections.Dictionary item)
    {
        string imageUrl = api.GetAlbumImageUrl(item);
        onSongChanged?.Invoke(imageUrl);
    }

    private void OnRefreshNewState(double position, double duration, bool paused, float volume)
    {
        songPosition = position;
        songDuration = duration;
        isPaused = paused;
        this.volume = volume;
    }

    public async Task<Texture2D> GetCurrentAlbumImage()
    {
        return await api.GetCurrentAlbumImage();
    }

    public async Task<Texture2D> GetCurrentAlbumImage(string url)
    {
        return await api.GetAlbumImage(url);
    }

    private async void OnAccessTokenReceived(string token)
    {
        string tokenJson = Json.Stringify(token);
        webView.Call("eval", $"setAccessToken({tokenJson});");
        api.SetAccessToken(token);
        spotifyApiReady?.Invoke();
    }

    private async void OnServerStarted()
    {
        webView.Set("full_window_size", false);
        webView.Call("set_visible", true);
        webView.Call("set_size", new Vector2(1, 1));
        webView.Call("set_position", new Vector2(0, 0));

        webView.Call(
            "load_url",
            "http://127.0.0.1:8888/spotify.html"
        );

        if (auth.LoadRefreshToken())
        {
            await auth.RefreshAccessToken();
        }
        else
        {
            auth.RequestUserAuthorization();
        }
    }

    private async void OnCodeReceived(string code)
    {
        await auth.ExchangeCodeForToken(code);
    }

    public override void _Process(double delta)
    {
        if (!isPaused)
        {
            songPosition += delta * 1000.0;

            if (songPosition > songDuration)
                songPosition = songDuration;
        }
    }
}
