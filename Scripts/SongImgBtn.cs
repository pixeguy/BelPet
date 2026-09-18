using Godot;
using System;

public partial class SongImgBtn : TextureButtonMask
{
    [Export]
    private Sprite2D playBtn;
    [Export]
    private Sprite2D pauseBtn;
    private Sprite2D fadingIcon;
    private float fadeTimer = 0f;
    private float fadeDuration = 1f;

    private TextureRect songImg;
    private ShaderMaterial songMaterial;

    private float targetZoom = 1.0f;
    private float hoverZoom = 1.1f;
    private float zoomSpeed = 8f;

    public event Action onClicked;

    public override void _Ready()
    {
        base._Ready();
        songImg = GetParent<TextureRect>();
        songMaterial = songImg.Material as ShaderMaterial;

        playBtn.Modulate = new Color(1, 1, 1, 0);
        pauseBtn.Modulate = new Color(1, 1, 1, 0);
    }

    public void SetPlaybackState(bool isPaused)
    {
        if (fadingIcon != null) fadingIcon.Modulate = new Color(1, 1, 1, 0);

        fadingIcon = isPaused ? pauseBtn : playBtn;
        fadingIcon.Modulate = new Color(1, 1, 1, 1);
        fadeTimer = 0f;
    }

    public override void OnButtonPressed()
    {
        onClicked?.Invoke();
    }

    public override void OnButtonHover()
    {
        targetZoom = hoverZoom;
    }

    public override void OnButtonHoverAway()
    {
        targetZoom = 1.0f;
    }

    public override void _Process(double delta)
    {
        float currentZoom = (float)songMaterial.GetShaderParameter("zoom");
        float newZoom = Mathf.Lerp(currentZoom, targetZoom, (float)delta * zoomSpeed);
        songMaterial.SetShaderParameter("zoom", newZoom);


        if (fadingIcon != null)
        {
            fadeTimer += (float)delta;

            float t = fadeTimer / fadeDuration;
            float alpha = Mathf.Lerp(1f, 0f, t);

            fadingIcon.Modulate = new Color(1, 1, 1, alpha);

            if (fadeTimer >= fadeDuration)
            {
                fadingIcon.Modulate = new Color(1, 1, 1, 0);
                fadingIcon = null;
            }
        }
    }
}
