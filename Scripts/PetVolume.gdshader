shader_type canvas_item;

void vertex() {
	// Called for every vertex the material is visible on.
}

uniform float fill_amount : hint_range(0.0, 1.0) = 0.5;
uniform vec4 fill_color : source_color = vec4(0.2, 0.7, 1.0, 1.0);

uniform float white_threshold : hint_range(0.0, 1.0) = 0.9;

uniform float wave_height : hint_range(0.0, 0.1) = 0.015;
uniform float wave_speed : hint_range(0.0, 10.0) = 1.0;

// How close to 0/100 before snapping.
uniform float edge_snap : hint_range(0.0, 0.1) = 0.02;

void fragment()
{
    vec4 original = texture(TEXTURE, UV);

    bool is_white =
        original.r >= white_threshold &&
        original.g >= white_threshold &&
        original.b >= white_threshold;

    float water_level = 1.0 - fill_amount;

    float wave1 =
        sin(UV.x * 7.3 + TIME * wave_speed)
        * wave_height;

    float wave2 =
        sin(UV.x * 13.7 - TIME * wave_speed * 0.63 + 1.8)
        * wave_height * 0.45;

    float wave3 =
        sin(UV.x * 3.1 + TIME * wave_speed * 0.27 + 4.2)
        * wave_height * 0.35;

    float wave4 =
        sin(UV.x * 21.4 + TIME * wave_speed * 1.31)
        * wave_height * 0.12;

    float wave = wave1 + wave2 + wave3 + wave4;

    bool inside_fill;

    if (fill_amount <= edge_snap)
    {
        inside_fill = false;
    }
    else if (fill_amount >= 1.0 - edge_snap)
    {
        inside_fill = true;
    }
    else
    {
        inside_fill =
            UV.y >= water_level + wave;
    }

    if (is_white && inside_fill)
    {
        COLOR = vec4(
            fill_color.rgb,
            original.a
        );
    }
    else
    {
        COLOR = original;
    }
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
