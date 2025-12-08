#version 330 core

layout (location = 0) in vec3 in_position;
layout (location = 1) in vec3 in_color;

out vec3 v_color;

void main()
{
    gl_Position = vec4(in_position.x, -in_position.y, in_position.z, 1.0);
    v_color = in_color;
}