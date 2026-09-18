using Godot;
using System;

public partial class MovingBtn : Button
{
    public enum MoveDirection
    {
        Left,
        Right
    }

    [Export]
    private MoveDirection direction = MoveDirection.Left;
    [Export]
    private float moveDistance = 20f;
    [Export]
    private float moveDuration = 0.2f;
    private Vector2 originalPosition;
    private Vector2 originalScale;

    private float animationProgress = 0f;
    private bool isHovered = false;
    public event Action onClicked;

    public override void _Ready()
    {
        originalPosition = Position;
        originalScale = Scale;

        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        Pressed += OnPressed;
    }

    public override void _Process(double delta)
    {
        float targetProgress = isHovered ? 1f : 0f;

        animationProgress = Mathf.MoveToward(animationProgress, targetProgress, (float)delta / moveDuration);

        // Ease out cubic: fast at start, slow at end
        float t = 1f - Mathf.Pow(1f - animationProgress, 3f);
        float directionMultiplier = direction == MoveDirection.Left ? -1f : 1f;
        Vector2 movedPosition = originalPosition + new Vector2( moveDistance * directionMultiplier, 0);
        Position = originalPosition.Lerp(movedPosition, t);
    }

    private void OnMouseEntered()
    {
        isHovered = true;
    }

    private void OnMouseExited()
    {
        isHovered = false;
    }

    private void OnPressed()
    {
        onClicked?.Invoke();

        Tween tween = CreateTween();
        tween.TweenProperty(this, "scale", originalScale * 1.08f, 0.08f).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(this, "scale", originalScale, 0.12f).SetEase(Tween.EaseType.Out);
    }
}