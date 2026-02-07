#version 450

layout(location = 0) in vec2 position;
layout(location = 1) in vec3 dir;
layout(location = 0) out vec2 uv;
layout(location = 1) out vec3 viewDir;
        
void main()
{
    gl_Position = vec4(position, 0, 1);
    uv = (position + 1) * 0.5;
    uv.y = 1 - uv.y;
    viewDir =  normalize(dir);
}