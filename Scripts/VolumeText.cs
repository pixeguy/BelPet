using Godot;

public partial class VolumeText : Label
{
    private float timeSinceChange = 0f;
    private string lastText = "";
    [Export]
    private float waitDuration = 1f;
    [Export]
    private float fadeSpeed = 0.5f;
    private const float visibleAlpha = 0.604f;

    public void SetVolumeText(string text)
    {
        Text = text;

        if (text != lastText)
        {
            lastText = text;
            timeSinceChange = 0f;

            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, visibleAlpha);
        }
    }

    public override void _Process(double delta)
    {
        timeSinceChange += (float)delta;

        if (timeSinceChange >= waitDuration)
        {
            float alpha = Mathf.MoveToward(Modulate.A, 0f, fadeSpeed * (float)delta);
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, alpha);
        }
    }
}