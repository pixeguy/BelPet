using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HttpClient = System.Net.Http.HttpClient;
using RandomNumberGenerator = System.Security.Cryptography.RandomNumberGenerator;


public partial class SpotifyAuth : Node
{
    private readonly HttpClient client = new HttpClient();

	string clientId = "4c71a49731214647be320f861247de78";
    string redirectUri = "http://127.0.0.1:8888/callback";
    string scope =
        "streaming " +
        "user-read-email " +
        "user-read-private " +
        "user-modify-playback-state " +
        "user-read-playback-state " +
        "user-read-currently-playing";
    string tokenUrl = "https://accounts.spotify.com/api/token";
    string saveTokenFile = "user://spotify_token.dat";
    string codeVerifier;

    private string accessToken;
    private string refreshToken;
    private int tokenExpiresIn;

    private DateTime accessTokenExpiresAt;
    private CancellationTokenSource refreshCancellation;

    public event Action<string> accessTokenReceived;
    public event Action authenticationRequired;

    public void RequestUserAuthorization()
    {
        int length = 50;
        codeVerifier = GenerateRandomString(length);
        string codeChallenge = sha256_hash(codeVerifier);

        var authUrl = new Uri("https://accounts.spotify.com/authorize");

        var parameters = new Dictionary<string, string>
        {
            {"response_type", "code" },
            {"client_id", clientId},
            {"scope", scope},
            {"code_challenge_method", "S256"},
            {"code_challenge", codeChallenge},
            {"redirect_uri", redirectUri},
        };

        authUrl = new Uri(authUrl + "?" + string.Join("&", parameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}")));
        OS.ShellOpen(authUrl.ToString());
    }

    public async Task ExchangeCodeForToken(string code)
    {
        var body = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            {"client_id", clientId },
            {"grant_type", "authorization_code"},
            {"code", code},
            {"redirect_uri", redirectUri},
            {"code_verifier", codeVerifier}
        });
        HttpResponseMessage response = await client.PostAsync(tokenUrl, body);
        string json = await response.Content.ReadAsStringAsync();

        Json jsonData = new Json();
        jsonData.Parse(json);
        Godot.Collections.Dictionary result = (Godot.Collections.Dictionary)jsonData.Data;

        accessToken = result["access_token"].AsString();
        refreshToken = result["refresh_token"].AsString();
        tokenExpiresIn = result["expires_in"].AsInt32();

        accessTokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenExpiresIn);
        ScheduleTokenRefresh();

        SaveRefreshToken();
        accessTokenReceived?.Invoke(accessToken);
    }

    public async Task RefreshAccessToken()
    {
        var body = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken },
            { "client_id", clientId }
        });
        HttpResponseMessage response = await client.PostAsync(tokenUrl, body);
        string json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            GD.PrintErr($"Failed to refresh Spotify token: {response.StatusCode}");
            HandleInvalidRefreshToken();
            return;
        }

        Json jsonData = new Json();
        jsonData.Parse(json);
        Godot.Collections.Dictionary result = (Godot.Collections.Dictionary)jsonData.Data;

        accessToken = result["access_token"].AsString();
        tokenExpiresIn = result["expires_in"].AsInt32();

        accessTokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenExpiresIn);
        ScheduleTokenRefresh();

        if (result.ContainsKey("refresh_token"))
        {
            refreshToken = result["refresh_token"].AsString();
            SaveRefreshToken();
        }

        accessTokenReceived?.Invoke(accessToken);
    }

    private void SaveRefreshToken()
    {
        using FileAccess file = FileAccess.Open(saveTokenFile, FileAccess.ModeFlags.Write);
        file.StoreString(refreshToken);
    }

    public bool LoadRefreshToken()
    {
        if (!FileAccess.FileExists(saveTokenFile))
            return false;

        using FileAccess file = FileAccess.Open(saveTokenFile, FileAccess.ModeFlags.Read);

        refreshToken = file.GetAsText();

        return true;
    }

    private async Task WaitForTokenRefresh(CancellationToken cancellationToken)
    {
        TimeSpan delay =
            accessTokenExpiresAt
            - DateTime.UtcNow
            - TimeSpan.FromMinutes(1);

        if (delay < TimeSpan.Zero)
            delay = TimeSpan.Zero;

        try
        {
            await Task.Delay(delay, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        await RefreshAccessToken();
    }

    private void ScheduleTokenRefresh()
    {
        // Cancel the previous scheduled refresh
        refreshCancellation?.Cancel();
        refreshCancellation?.Dispose();

        // Create a new one
        refreshCancellation = new CancellationTokenSource();

        _ = WaitForTokenRefresh(refreshCancellation.Token);
    }

    private void HandleInvalidRefreshToken()
    {
        refreshToken = null;

        if (FileAccess.FileExists(saveTokenFile))
        {
            DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(saveTokenFile));
        }

        authenticationRequired?.Invoke();
    }

    public string GenerateRandomString(int length)
	{
		string possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
		byte[] values = RandomNumberGenerator.GetBytes(length);
		string result = "";
		foreach (byte b in values)
		{
			int index = b % possible.Length;
			result += possible[index];
        }
		return result;
	}

    public string sha256_hash(String value)
    {
        using SHA256 sha256 = SHA256.Create();

        byte[] inputBytes = Encoding.UTF8.GetBytes(value);

        byte[] hashBytes = sha256.ComputeHash(inputBytes);

        return Convert.ToBase64String(hashBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
