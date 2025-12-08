#version 330 core

uniform float u_offset;

layout (location = 0) in vec3 in_position;
layout (location = 1) in vec3 in_color;

out vec3 v_color;

void main()
{
    gl_Position = vec4(in_position.x + u_offset, in_position.yz, 1.0);
    v_color = in_color;
}