#version 450

layout(location = 0) in vec3 position;

layout (set = 0, binding = 0) uniform M
{
    mat4 model;
} m;

layout (set = 0, binding = 1) uniform VP
{
    mat4 view;
    mat4 projection;
} vp;

void main()
{
    gl_Position = vp.projection * vp.view * m.model * vec4(position, 1.0);
}