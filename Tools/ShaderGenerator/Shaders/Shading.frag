#version 450

// glslc D://Rider//Project//ET//Tools//ShaderGenerator//Shaders//Shading.frag -o D://Rider//Project//ET//Bin//Shaders//Shading.frag.spv

layout(location = 0) in vec2 fragUV;
layout(location = 1) in vec3 fragWorldNormal;
layout(location = 2) in vec3 fragWorldPos;

layout(location = 0) out vec4 outColor;

layout (set = 0, binding = 2) uniform Light
{
    mat4 view;
    mat4 projection;
    vec3 dir;
    float intensity;
    vec4 color; // vec3
    vec4 worldPos; // vec3
} light;

layout (set = 0, binding = 3) uniform Camera
{
    vec4 worldPos;
} camera;

layout(set = 0, binding = 4) uniform texture2D shadowMap;
layout(set = 0, binding = 5) uniform sampler shadowMapSampler;

float Fresnel_Schlick(float cosTheta, float F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

float D_GGX(float cosThetaH, float alpha) 
{
    float alpha2 = alpha * alpha;
    float cosThetaH2 = cosThetaH * cosThetaH;
    return alpha2 / (3.14159f * pow(cosThetaH2 * (alpha2 - 1.0) + 1.0, 2.0));
}

float G_SchlickGGX(float cosThetaV, float alpha) 
{
    float r = (alpha + 1.0) * cosThetaV;
    float k = alpha * alpha / 2.0;
    return cosThetaV / (cosThetaV * (1.0 - k) + k);
}

vec3 PBR_GGX(vec3 albedo, float metallic, float roughness, vec3 normal /*法线*/, vec3 viewDir/*物体指向相机*/, vec3 lightDir/*物体指向光源*/, vec3 Li/*光源颜色*强度*/)
{
    float F0 = 0.04;
    F0 = mix(F0, metallic, metallic);
    vec3 halfDir = normalize(lightDir + viewDir);
    
    vec3 h = normalize(lightDir + viewDir);
    float cosThetaH = dot(normal, halfDir);
    
    float F = Fresnel_Schlick(max(cosThetaH, 0.0), F0);

    float D = D_GGX(cosThetaH, roughness);

    float G = G_SchlickGGX(max(dot(normal, viewDir), 0.0), roughness) * G_SchlickGGX(max(dot(normal, lightDir), 0.0), roughness);

    vec3 specular = vec3(D * F * G / (4.0 * max(dot(normal, viewDir), 0.0) * max(dot(normal, lightDir), 0.0)));
    vec3 diffuse = (1.0 - F) * (1.0 - metallic) * albedo / 3.14159f;

    return diffuse + specular;
}

void main() 
{
    vec4 shadowCoord = light.projection * light.view * vec4(fragWorldPos, 1.0);
    shadowCoord /= shadowCoord.w;
    // 这里要注意, xy的范围是[-1, 1], z是[0, 1]
    shadowCoord.xy = shadowCoord.xy * 0.5 + 0.5;

    float currentDepth = shadowCoord.z;
    
    // Hard Shadow
    /*float shadowDepth = texture(sampler2D(shadowMap, shadowMapSampler), shadowCoord.xy).r;
    
    float bias = 0.01;
    // 同时这里注意要有=
    float shadow = currentDepth - bias >= shadowDepth ? 0.3 : 1.0;*/

    // PCF
    /**float shadow = 0.0;
    int radius = 4;
    float bias = 0.01;
    vec2 texel_size = 1.0 / vec2(textureSize(sampler2D(shadowMap, shadowMapSampler), 0)); // 每个像素的大小
    for (int x = -radius; x <= radius; ++x)
    {
        for (int y = -radius; y <= radius; ++y)
        {
            float pcf_depth = texture(sampler2D(shadowMap, shadowMapSampler), vec2(shadowCoord.xy + vec2(x, y) * texel_size)).r;
            shadow += (currentDepth - bias) > pcf_depth ? 0.0 : 1.0;
        }
    }
    shadow /= pow((1+radius*2),2.0);*/
    
    // 计算阴影要注意 ShadowMap值越大里光源越远, 这和很多文章不同, 他们是越大离得越近
    
    // PCSS
    // 1.遮挡物平均深度
    float blockerSumDepth = 0.0;
    int blockerCount = 0;
    int blockerSearchArea = 4;
    vec2 texelSize = 1.0 / vec2(textureSize(sampler2D(shadowMap, shadowMapSampler), 0)); // 每个像素的大小
    for (float x = -blockerSearchArea; x <= blockerSearchArea; x++)
    {
        for (float y = -blockerSearchArea; y <= blockerSearchArea; y++)
        {
            float blockerDepth = texture(sampler2D(shadowMap, shadowMapSampler), vec2(shadowCoord.xy + vec2(x, y) * texelSize)).r;
            if (blockerDepth > currentDepth)
            {
                blockerSumDepth += blockerDepth;
                ++blockerCount;
            }
        }   
    }
    float avgBlockerDepth = blockerCount > 0 ? blockerSumDepth / float(blockerCount) : 0.0;
    // 2.计算PCF采样范围(遮挡物半径)
    float lightArea = 1.0;
    float blockerRadius = avgBlockerDepth <= 0.0 ? 0.0 : (currentDepth - avgBlockerDepth) * (lightArea /avgBlockerDepth );
    // 3.PCF
    float shadowSumDepth = 0.0;
    int sampleCount = 150;
    float bias = 0.01;
    for (int i = 0; i < sampleCount; ++i)
    {
        vec2 offset = vec2(cos(float(i) * 2.0 * 3.1415926 / float(sampleCount)), sin(float(i) * 2.0 * 3.1415926 / float(sampleCount))) * blockerRadius;
        float pcssDepth = texture(sampler2D(shadowMap, shadowMapSampler), vec2(shadowCoord.xy + offset)).r;
        shadowSumDepth += /*currentDepth > pcssDepth ? 0.0 : 1.0;*/(currentDepth - bias) > pcssDepth ? 0.0 : 1.0;
    }
    float shadow = shadowSumDepth / sampleCount;
    
    // 漫反射光照
    vec3 normal = normalize(fragWorldNormal);
    float ndl = max(dot(normal, -light.dir), 0.0);
    vec3 baseColor = /*texture(img, fragUV).xyz;*/ vec3(1.0);
    vec3 diffuse = baseColor * light.color.rgb * ndl * light.intensity;
    
    vec3 v = normalize(camera.worldPos.xyz - fragWorldPos);
    vec3 l = normalize(light.worldPos.xyz - fragWorldPos);
    vec3 pbr = PBR_GGX(vec3(1), 0.01, 0.98, normal, v, l, vec3(1) * 0.8);
    
    outColor = vec4(pbr * shadow, 1.0);
}