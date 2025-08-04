#version 450

layout(location = 0) out vec2 fragUV;
layout(location = 1) out vec3 fragWorldNormal;
layout(location = 2) out vec3 fragWorldPos;

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 uv;
layout(location = 2) in vec3 normal;

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
    fragUV = uv;
    fragWorldNormal = normalize(transpose(inverse(mat3(m.model))) * normal);
    fragWorldPos = (m.model * vec4(position, 1.0)).xyz;
    gl_Position = vp.projection * vp.view * m.model * vec4(position, 1.0);
}