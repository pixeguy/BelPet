using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;

public partial class DesktopPet : Node2D
{
	[Export]
	private CollisionShape2D renderCollider;

    [Export]
    private CollisionShape2D dragCollider;
    private Area2D dragArea;

    [Export]
	private SpotifyManager spotify;
	[Export]
	private SongUiManager songUi;
    [Export]
    private Texture2D spotifyLogo;
    private bool isProgressClicked = false;

    private bool isDragging = false;
    private Vector2I dragOffset;

    [Export]
    private Texture2D normalSprite;
    [Export]
    private Texture2D grabbedSprite;

    [Export]
    private VolumeText volumeLabel;
    [Export]
    private Sprite2D petSprite;
    private ShaderMaterial petMaterial;
    private float displayedVolume = 0.5f;
    private const float volumeStep = 0.05f;

    private float verticalVelocity = 0f;
    [Export]
    private float gravity = 1800f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		Window window = GetWindow();
		window.Borderless = true;
		window.Transparent = true;
		window.AlwaysOnTop = true;

        petMaterial = petSprite.Material as ShaderMaterial;

        dragArea = dragCollider.GetParent<Area2D>();
        dragArea.InputEvent += OnPetInput;
        dragArea.MouseEntered += OnDragAreaEntered;
        dragArea.MouseExited += OnDragAreaExited;

        UpdateClickableRegion();

		spotify.spotifyApiReady += RegisterSong;
        spotify.onSongChanged += RegisterSong;
        spotify.onPlayBackChanged += OnPlayBackChanged;

        songUi.onProgressUIClicked += OnProgressUIClicked;
        songUi.onProgressUIStopClicked += OnProgressUIStopClicked;
        songUi.onSongBtnClicked += OnSongBtnClicked;
        songUi.onNextSongBtnClicked += OnNextSongBtnClicked;
        songUi.onPrevSongBtnClicked += OnPrevSongBtnClicked;
    }

	public async void RegisterSong()
    {
        Texture2D img = await spotify.GetCurrentAlbumImage();
        if (img == null)
            img = spotifyLogo;
        songUi.SetSongSprite(img);
    }

	public async void RegisterSong(string url)
    {
        Texture2D img = await spotify.GetCurrentAlbumImage(url);
        if (img == null)
            img = spotifyLogo;
        songUi.SetSongSprite(img);
    }

    private bool IsMouseOverNonDraggableUI()
    {
        Control hovered = GetViewport().GuiGetHoveredControl();
        return hovered != null && hovered.IsInGroup("BlocksPetDragging");
    }
    private void OnDragAreaEntered()
    {
        Input.SetDefaultCursorShape(Input.CursorShape.PointingHand);
    }

    private void OnDragAreaExited()
    {
        Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
    }

    private void OnPetInput(
		Node viewport,
		InputEvent @event,
		long shapeIdx)
    {
        if (@event is InputEventMouseButton mouseButton && !IsMouseOverNonDraggableUI())
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (mouseButton.Pressed)
                {
                    if (IsMouseOverNonDraggableUI())
                        return;

                    isDragging = true;
                    Vector2I mousePosition = DisplayServer.MouseGetPosition();
                    dragOffset = mousePosition - GetWindow().Position;

                    petSprite.Texture = grabbedSprite;
                }
                else
                {
                    isDragging = false;
                    petSprite.Texture = normalSprite;
                }
            }

            if (mouseButton.Pressed)
            {
                if (mouseButton.ButtonIndex == MouseButton.WheelUp)
                {
                    spotify.ChangeVolume(volumeStep);
                }
                else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
                {
                    spotify.ChangeVolume(-volumeStep);
                }
            }
        }
    }

    public void OnNextSongBtnClicked()
    {
        spotify.NextSong();
    }

    public void OnPrevSongBtnClicked()
    {
        spotify.PrevSong();
    }

    public void OnPlayBackChanged(bool paused)
    {
        songUi.SetPlaybackState(paused);
    }

    public void OnSongBtnClicked()
    {
        spotify.PlayOrPauseSong();
    }

    public void OnProgressUIClicked(float progress)
    {
        isProgressClicked = true;
        songUi.SetSongProgress(progress, 100);
    }

    public void OnProgressUIStopClicked(float progress)
    {
        isProgressClicked = false;
        isDragging = false;
        spotify.SetSongProgress(progress / 100);
    }

    private void UpdateClickableRegion()
	{
		if (renderCollider.Shape is not RectangleShape2D rectangle)
		{
			GD.PrintErr("Click collider must currently be a RectangleShape2D.");
			return;
		}

		Vector2 extents = rectangle.Size / 2f;

		Vector2[] localCorners =
		{
		new Vector2(-extents.X, -extents.Y), // Top left
		new Vector2( extents.X, -extents.Y), // Top right
		new Vector2( extents.X,  extents.Y), // Bottom right
		new Vector2(-extents.X,  extents.Y), // Bottom left
	};

		Transform2D transform = renderCollider.GetGlobalTransformWithCanvas();

		Vector2[] windowCorners = new Vector2[4];

		for (int i = 0; i < 4; i++)
		{
			windowCorners[i] = transform * localCorners[i];
		}

		GetWindow().MousePassthroughPolygon = windowCorners;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        if (!isProgressClicked)
		    songUi.SetSongProgress((float)spotify.SongProgress, (float)spotify.SongDuration);

        displayedVolume = Mathf.Lerp(displayedVolume, spotify.Volume, (float)delta * 8f);
        petMaterial?.SetShaderParameter("fill_amount", displayedVolume);
        volumeLabel.SetVolumeText($"{Mathf.RoundToInt(spotify.Volume * 100)}%");

        Vector2I windowPosition = GetWindow().Position;

        if (isDragging && !isProgressClicked)
        {
            Vector2I mousePosition = DisplayServer.MouseGetPosition();
            windowPosition = mousePosition - dragOffset;
            verticalVelocity = 0f;
        }
        else
        {
            verticalVelocity += gravity * (float)delta;
            windowPosition.Y += Mathf.RoundToInt(verticalVelocity * (float)delta);
        }

        int screen = DisplayServer.WindowGetCurrentScreen(GetWindow().GetWindowId());
        Rect2I usableRect = DisplayServer.ScreenGetUsableRect(screen);
        int bottom = usableRect.Position.Y + usableRect.Size.Y;

        RectangleShape2D rectangle = dragCollider.Shape as RectangleShape2D;
        Transform2D transform = dragCollider.GetGlobalTransformWithCanvas();
        Vector2 bottomCenter = transform * new Vector2(0, rectangle.Size.Y / 2f);
        float petBottom = bottomCenter.Y;

        if (windowPosition.Y + petBottom > bottom)
        {
            windowPosition.Y = Mathf.RoundToInt(bottom - petBottom);
            verticalVelocity = 0f;
        }

        GetWindow().Position = windowPosition;
    }
}
