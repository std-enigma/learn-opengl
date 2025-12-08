#version 330 core

uniform float u_offset;

layout (location = 0) in vec3 in_position;
layout (location = 1) in vec3 in_color;

out vec3 v_color;

void main()
{
    gl_Position = vec4(in_position.x + u_offset, in_position.yz, 1.0);
    v_color = in_position;
}

// Question:
// Output the vertex position to the fragment shader and set the fragment's
// color equal to this vertex position. Once you do this, why is the
// bottom-left side of the triangle black?

// Answer:
// The bottom-left vertex of the triangle appears black because its position is
// (-0.5, -0.5, 0.0), and since we're using these coordinates directly as a color.
// Since color channels are clamped to the [0.0, 1.0] range, the negative
// values become 0.0. This produces a final color of (0.0, 0.0, 0.0), which is
// black.