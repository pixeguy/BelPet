using Godot;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GodotDictionary = Godot.Collections.Dictionary;
using HttpClient = System.Net.Http.HttpClient;

public partial class SpotifyAPI : Node
{
    private readonly HttpClient client = new HttpClient();
    private string accessToken;
	private string spotifyApiUrl = "https://api.spotify.com/v1/me/player/";

    string currentTrackId;

    private bool isPolling = false;
    private readonly TimeSpan pollingInterval = TimeSpan.FromSeconds(3);

    private CancellationTokenSource volumeDebounce;

    public event Action<GodotDictionary> OnFetchNewState;
    public event Action<double, double, bool, float> OnRefreshNewState;

	public void SetAccessToken(string token)
	{
		this.accessToken = token;
        StartFetchTimer();
    }

    public void SetTrackId(string id)
    {
        this.currentTrackId = id;
    }

	public async Task Pause()
	{
		var request = new HttpRequestMessage(HttpMethod.Put, spotifyApiUrl + "pause");
		request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
		HttpResponseMessage response = await client.SendAsync(request);
    }

    public async Task Play()
    {
        var request = new HttpRequestMessage(HttpMethod.Put, spotifyApiUrl + "play");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        HttpResponseMessage response = await client.SendAsync(request);
    }

    public async Task SetSongProgress(double progress)
    {
        long position = (long)progress;
        var request = new HttpRequestMessage(HttpMethod.Put, spotifyApiUrl + $"seek?position_ms={position}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        HttpResponseMessage response = await client.SendAsync(request);
    }

    public async Task SkipToNextSong()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, spotifyApiUrl + "next");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        HttpResponseMessage response = await client.SendAsync(request);
    }

    public async Task SkipToPrevSong()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, spotifyApiUrl + "previous");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        HttpResponseMessage response = await client.SendAsync(request);
    }

    public async void ScheduleVolumeChange(float vol)
    {
        volumeDebounce?.Cancel();
        volumeDebounce?.Dispose();

        volumeDebounce = new CancellationTokenSource();

        try
        {
            await Task.Delay(250, volumeDebounce.Token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        await SetVolume(vol);
    }

    public async Task SetVolume(float vol)
    {
        int volume = Mathf.RoundToInt(vol);
        var request = new HttpRequestMessage(HttpMethod.Put, spotifyApiUrl + $"volume?volume_percent={volume}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        HttpResponseMessage response = await client.SendAsync(request);
    }

    public async Task<GodotDictionary> GetCurrentPlayingTrack()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, spotifyApiUrl);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        HttpResponseMessage response = await client.SendAsync(request);

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return null;
        }

        string json = await response.Content.ReadAsStringAsync();
        Json jsonData = new Json();
		jsonData.Parse(json);

        GodotDictionary result = (GodotDictionary)jsonData.Data;

        if (!result.ContainsKey("item") || result["item"].VariantType == Variant.Type.Nil)
        {
            return null;
        }
        return result;
    }

    public (double position, double duration, bool paused, float volume) GetCurrentPlaybackState(GodotDictionary result)
    {
        if (result == null)
            return (0, 0, true, 0);

        GodotDictionary item = (GodotDictionary)result["item"];
        GodotDictionary device = (GodotDictionary)result["device"];

        double position = result["progress_ms"].AsDouble();
        double duration = item["duration_ms"].AsDouble();
        bool isPlaying = result["is_playing"].AsBool();
        float volume = device["volume_percent"].AsSingle() / 100f;

        return (position, duration, !isPlaying, volume);
    }

    public async Task<Texture2D> GetCurrentAlbumImage()
    {
        GodotDictionary result = await GetCurrentPlayingTrack();

        if (result == null)
            return null;

        GodotDictionary item = (GodotDictionary)result["item"];

        if (item == null)
            return null;

        string url = GetAlbumImageUrl(item);
        return await GetAlbumImage(url);
    }

    public string GetAlbumImageUrl(GodotDictionary item)
    {
        GodotDictionary album = (GodotDictionary)item["album"];
        Godot.Collections.Array images = (Godot.Collections.Array)album["images"];
        GodotDictionary image = (GodotDictionary)images[0];

        return image["url"].AsString();
    }

    public async Task<Texture2D> GetAlbumImage(string imageUrl)
    {
        byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);
        Image img = new Image();
        Error error = img.LoadJpgFromBuffer(imageBytes);
        if (error != Error.Ok)
        {
            GD.PrintErr("Failed to load Spotify image.");
            return null;
        }
        return ImageTexture.CreateFromImage(img);
    }

    public void StopFetchTimer()
    {
        isPolling = false;
    }

    public void StartFetchTimer()
    {
        if (isPolling)
            return;

        isPolling = true;
        _ = PollLoop();
    }

    private async Task PollLoop()
    {
        while (isPolling)
        {
            try
            {
                await OnFetchTimerTimeout();
            }
            catch (Exception e)
            {
                GD.PrintErr($"Spotify poll failed: {e}");
            }
            await Task.Delay(pollingInterval);
        }
    }

    public async Task RefreshNow()
    {
        await OnFetchTimerTimeout();
    }

    public async Task OnFetchTimerTimeout()
    {
        GodotDictionary result = await GetCurrentPlayingTrack();

        if (result == null)
            return;

        GodotDictionary item = (GodotDictionary)result["item"];

        var playbackState = GetCurrentPlaybackState(result);
        OnRefreshNewState?.Invoke(playbackState.position, playbackState.duration, playbackState.paused, playbackState.volume);

        if (item == null)
            return;

        string trackId = item["id"].AsString();
        if (trackId == currentTrackId)
            return;

        currentTrackId = trackId;
        OnFetchNewState?.Invoke(item);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (Input.IsActionJustPressed("jump"))
        {
        }
	}
}
