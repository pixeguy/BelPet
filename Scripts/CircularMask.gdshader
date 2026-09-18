shader_type canvas_item;

uniform float zoom : hint_range(1.0, 2.0) = 1.0;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment()
{
    vec2 centered_uv = UV - vec2(0.5);

    // Keep the physical circular crop exactly the same
    if (length(centered_uv) > 0.5)
    {
        discard;
    }

    // Zoom the image around its center
    vec2 zoomed_uv = centered_uv / zoom + vec2(0.5);

    COLOR = texture(TEXTURE, zoomed_uv);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
