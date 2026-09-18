shader_type canvas_item;

uniform float ring_thickness : hint_range(0.01, 0.5) = 0.08;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment()
{
    vec2 centered_uv = UV - vec2(0.5);
    float distance_from_center = length(centered_uv);

    float outer_radius = 0.5;
    float inner_radius = outer_radius - ring_thickness;

    // Remove everything outside the ring
    if (distance_from_center > outer_radius ||
        distance_from_center < inner_radius)
    {
        discard;
    }

    COLOR = texture(TEXTURE, UV);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
