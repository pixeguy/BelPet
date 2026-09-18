using Godot;
using System;

public partial class SongProgressUi : TextureProgressBar
{
    [Export]
    private float normalThickness = 0.04f;
    [Export]
    private float hoverThickness = 0.08f;
    [Export]
    private float hoverDistance = 40f;
    [Export]
    private float lerpSpeed = 8f;
    private bool mouseIsNear;

    private ShaderMaterial shaderMaterial;

    private bool isClickingProgress = false;
    public event Action<float> onClicked;
    public event Action<float> onClickedRelease;

    public override void _Ready()
    {
        shaderMaterial = Material as ShaderMaterial;
    }

    public override bool _HasPoint(Vector2 point)
    {
        Vector2 center = Size / 2f;
        float distance = point.DistanceTo(center);
        float outerRadius = Mathf.Min(Size.X, Size.Y) / 2f;
        float thickness = (float)shaderMaterial.GetShaderParameter("ring_thickness");
        float thicknessPixels = thickness * Mathf.Min(Size.X, Size.Y);
        float innerRadius = outerRadius - thicknessPixels;

        return distance >= innerRadius && distance <= outerRadius;
    }

    public override void _Process(double delta)
    {
        Vector2 mousePosition = GetLocalMousePosition();
        Vector2 center = Size / 2f;

        float mouseDistance = mousePosition.DistanceTo(center);

        float radius = Mathf.Min(Size.X, Size.Y) / 2f;
        float distanceFromRing = Mathf.Abs(mouseDistance - radius);

        mouseIsNear = distanceFromRing <= hoverDistance;

        float targetThickness = (mouseIsNear || isClickingProgress) ? hoverThickness : normalThickness;
        float currentThickness = (float)shaderMaterial.GetShaderParameter("ring_thickness");
        float newThickness = Mathf.Lerp(currentThickness, targetThickness, (float)delta * lerpSpeed);

        shaderMaterial.SetShaderParameter("ring_thickness", newThickness);


        if (isClickingProgress)
        {
            float progress = GetProgressFromMouse(GetLocalMousePosition());
            onClicked?.Invoke(progress * 100);
        }
    }

    private float GetProgressFromMouse(Vector2 mousePosition)
    {
        Vector2 center = Size / 2f;
        Vector2 direction = mousePosition - center;

        float angle = Mathf.Atan2(direction.Y, direction.X);

        angle += Mathf.Pi / 2f;

        if (angle < 0)
            angle += Mathf.Tau;

        return angle / Mathf.Tau;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton &&
            mouseButton.ButtonIndex == MouseButton.Left)
        {
            Vector2 mousePosition = mouseButton.Position;
            Vector2 center = Size / 2f;

            float distance = mousePosition.DistanceTo(center);

            float outerRadius = Mathf.Min(Size.X, Size.Y) / 2f;
            float thickness = (float)shaderMaterial.GetShaderParameter("ring_thickness");
            float thicknessPixels = thickness * Mathf.Min(Size.X, Size.Y);
            float innerRadius = outerRadius - thicknessPixels;

            bool insideRing = distance >= innerRadius && distance <= outerRadius;

            // click
            if (mouseButton.Pressed && insideRing)
            {
                isClickingProgress = true;
                float progress = GetProgressFromMouse(mousePosition);
                onClicked?.Invoke(progress * 100);
            }

            // release
            else if (!mouseButton.Pressed && isClickingProgress)
            {
                isClickingProgress = false;
                float progress = GetProgressFromMouse(mousePosition);
                onClickedRelease?.Invoke(progress * 100);
            }
        }
    }
}
