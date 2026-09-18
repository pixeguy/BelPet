using Godot;
using System;

[Tool]
public partial class CircleButton : Button
{
    [Export]
    private CollisionShape2D hitboxPreview;

    public override bool _HasPoint(Vector2 point)
    {
        if (hitboxPreview?.Shape is not CircleShape2D circle)
            return false;

        Vector2 center = hitboxPreview.Position;
        float radius = circle.Radius;

        return point.DistanceTo(center) <= radius;
    }
}
