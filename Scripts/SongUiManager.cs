using Godot;
using System;

public partial class SongUiManager : Node
{
    [Export]
    private TextureRect songSprite;
    [Export]
    private TextureProgressBar songProgress;
    [Export]
    private SongProgressUi progressUi;
    [Export]
    private SongImgBtn songBtn;
    [Export]
    private MovingBtn nextSongBtn;
    [Export]
    private MovingBtn prevSongBtn;
    public event Action<float> onProgressUIClicked;
    public event Action<float> onProgressUIStopClicked;
    public event Action onSongBtnClicked;
    public event Action onNextSongBtnClicked;
    public event Action onPrevSongBtnClicked;

    public override void _Ready()
    {
        progressUi.onClicked += OnProgressClicked;
        progressUi.onClickedRelease += OnProgressClickedRelease;
        songBtn.onClicked += OnSongClicked;
        nextSongBtn.onClicked += OnNextSongClicked;
        prevSongBtn.onClicked += OnPrevSongClicked;
    }

    public void OnNextSongClicked()
    {
        onNextSongBtnClicked?.Invoke();
    }

    public void OnPrevSongClicked()
    {
        onPrevSongBtnClicked?.Invoke();
    }

    public void SetPlaybackState(bool isPaused)
    {
        songBtn.SetPlaybackState(isPaused);
    }

    public void OnSongClicked()
    {
        onSongBtnClicked?.Invoke();
    }

    public void OnProgressClicked(float progress)
    {
        onProgressUIClicked?.Invoke(progress);
    }

    public void OnProgressClickedRelease(float progress)
    {
        onProgressUIStopClicked?.Invoke(progress);
    }

    public void SetSongSprite(Texture2D texture)
    {
        songSprite.Texture = texture;
    }

    public void SetSongProgress(float current, float max)
    {
        if (max <= 0)
            return;

        float percent = (current / max) * 100;
        songProgress.Value = percent;
    }
}
