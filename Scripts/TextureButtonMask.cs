using Godot;

public partial class TextureButtonMask : TextureButton
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        Image image = TextureNormal.GetImage();

        Bitmap mask = new Bitmap();
        mask.CreateFromImageAlpha(image);

        TextureClickMask = mask;
        SelfModulate = new Color(1, 1, 1, 0);
        Pressed += OnButtonPressed;
        MouseEntered += OnButtonHover;
        MouseExited += OnButtonHoverAway;
    }

    public virtual void OnButtonPressed()
    {
    }

    public virtual void OnButtonHover()
    {
    }

    public virtual void OnButtonHoverAway()
    {
    }
}
