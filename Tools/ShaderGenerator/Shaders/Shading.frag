#version 450

// glslc D://Rider//Project//ET//Tools//ShaderGenerator//Shaders//Shading.frag -o D://Rider//Project//ET//Bin//Shaders//Shading.frag.spv
#define PI 3.14159f

layout(location = 0) in vec2 fragUV;
layout(location = 1) in vec3 fragWorldNormal;
layout(location = 2) in vec3 fragWorldPos;

layout(location = 0) out vec4 outColor;
layout(location = 1) out vec4 outNormal;
layout(location = 2) out vec4 outPosition;

layout (set = 0, binding = 2) uniform Light
{
    mat4 view;
    mat4 projection;
    vec3 dir;
    float intensity;
    vec4 color;
    vec4 worldPos;
} light;

layout (set = 0, binding = 3) uniform Camera
{
    mat4 view;
    mat4 projection;
    vec4 worldPos;
} camera;

layout(set = 0, binding = 4) uniform texture2D shadowMap;
layout(set = 0, binding = 5) uniform sampler shadowMapSampler;

layout(set = 0, binding = 6) uniform textureCube environmentMap;
layout(set = 0, binding = 7) uniform sampler environmentMapSampler;

// Shadow
//////////////////////////////////////////////////////////////////////////////////
float PCF(vec2 uv, float currentDepth)
{
    float shadow = 0.0;
    int radius = 4;
    float bias = 0.01;
    vec2 texel_size = 1.0 / vec2(textureSize(sampler2D(shadowMap, shadowMapSampler), 0)); // 每个像素的大小
    for (int x = -radius; x <= radius; ++x)
    {
        for (int y = -radius; y <= radius; ++y)
        {
            float pcf_depth = texture(sampler2D(shadowMap, shadowMapSampler), vec2(uv + vec2(x, y) * texel_size)).r;
            shadow += (currentDepth - bias) > pcf_depth ? 0.0 : 1.0;
        }
    }
    shadow /= pow((1+radius*2),2.0);
    return shadow;
}

float PCSS(vec2 uv, float currentDepth)
{
    // 1.遮挡物平均深度
    float blockerSumDepth = 0.0;
    int blockerCount = 0;
    int blockerSearchArea = 4;
    vec2 texelSize = 1.0 / vec2(textureSize(sampler2D(shadowMap, shadowMapSampler), 0)); // 每个像素的大小
    for (float x = -blockerSearchArea; x <= blockerSearchArea; x++)
    {
        for (float y = -blockerSearchArea; y <= blockerSearchArea; y++)
        {
            float blockerDepth = texture(sampler2D(shadowMap, shadowMapSampler), vec2(uv + vec2(x, y) * texelSize)).r;
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
        float pcssDepth = texture(sampler2D(shadowMap, shadowMapSampler), vec2(uv + offset)).r;
        shadowSumDepth += (currentDepth - bias) > pcssDepth ? 0.0 : 1.0;
    }
    float shadow = shadowSumDepth / sampleCount;
    return shadow;
}

// PBR
//////////////////////////////////////////////////////////////////////////////////
vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (vec3(1.0) - F0) * pow(1.0 - cosTheta, 5.0);
}
float D_GGX_TR(vec3 N, vec3 H, float a)
{
    float a2     = a*a;
    float NdotH  = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;

    float nom    = a2;
    float denom  = (NdotH2 * (a2 - 1.0) + 1.0);
    denom        = PI * denom * denom;

    return nom / denom;
}
float GeometrySchlickGGX(float NdotV, float k)
{
    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}
float GeometrySmith(vec3 N, vec3 V, vec3 L, float k)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx1 = GeometrySchlickGGX(NdotV, k);
    float ggx2 = GeometrySchlickGGX(NdotL, k);

    return ggx1 * ggx2;
}
vec3 PBR_GGX(vec3 albedo, float metallic, float roughness, vec3 normal /*法线*/, vec3 viewDir/*物体指向相机*/, vec3 lightDir/*物体指向光源*/, vec3 lightColor/*光源颜色*强度*/)
{
    vec3 F0 = mix(vec3(0.04), albedo, metallic);
    vec3 halfDir = normalize(lightDir + viewDir);

    vec3 F = fresnelSchlick(max(dot(halfDir, viewDir), 0.0), F0);

    float D = D_GGX_TR(normal, halfDir, roughness);

    float G = GeometrySmith(normal, viewDir, lightDir, roughness);

    vec3 nominator    = D * G * F;
    float NdotV = max(dot(normal, viewDir), 0.0);
    float NdotL = max(dot(normal, lightDir), 0.0);
    float denominator = 4.0 * NdotV * NdotL + 0.001;
    vec3 specular     = nominator / denominator;

    float distance    = length(lightDir);
    float attenuation = 1.0 / (distance * distance);
    vec3 radiance     = lightColor * attenuation;

    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - metallic;

    vec3 Lo = (kD * albedo / PI + specular) * radiance * NdotL;

    return Lo;
}

// Enviroment
//////////////////////////////////////////////////////////////////////////////////
// 1.IrradianceMap
vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(1.0 - cosTheta, 5.0);
}
vec3 IrradianceMap(vec3 normal)
{
    vec3 irradiance = vec3(0.0);
    vec3 up    = vec3(0.0, 1.0, 0.0);
    vec3 right = normalize(cross(up, normal));
    up         = normalize(cross(normal, right));
    float sampleDelta = 0.025;
    float nrSamples = 0.0;
    for(float phi = 0.0; phi < 2.0 * PI; phi += sampleDelta)
    {
        for(float theta = 0.0; theta < 0.5 * PI; theta += sampleDelta)
        {
            // spherical to cartesian (in tangent space)
            vec3 tangentSample = vec3(sin(theta) * cos(phi),  sin(theta) * sin(phi), cos(theta));
            // tangent space to world
            vec3 sampleVec = tangentSample.x * right + tangentSample.y * up + tangentSample.z * normal;

            irradiance += texture(samplerCube(environmentMap, environmentMapSampler), sampleVec).rgb * cos(theta) * sin(theta);
            nrSamples++;
        }
    }
    irradiance = PI * irradiance * (1.0 / float(nrSamples));
    return irradiance;
}
float RadicalInverse_VdC(uint bits)
{
    bits = (bits << 16u) | (bits >> 16u);
    bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
    bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
    bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
    bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
    return float(bits) * 2.3283064365386963e-10; // / 0x100000000
}
vec2 Hammersley(uint i, uint N)
{
    return vec2(float(i)/float(N), RadicalInverse_VdC(i));
}
vec3 ImportanceSampleGGX(vec2 Xi, vec3 N, float roughness)
{
    float a = roughness*roughness;

    float phi = 2.0 * PI * Xi.x;
    float cosTheta = sqrt((1.0 - Xi.y) / (1.0 + (a*a - 1.0) * Xi.y));
    float sinTheta = sqrt(1.0 - cosTheta*cosTheta);

    // from spherical coordinates to cartesian coordinates
    vec3 H;
    H.x = cos(phi) * sinTheta;
    H.y = sin(phi) * sinTheta;
    H.z = cosTheta;

    // from tangent-space vector to world-space sample vector
    vec3 up        = abs(N.z) < 0.999 ? vec3(0.0, 0.0, 1.0) : vec3(1.0, 0.0, 0.0);
    vec3 tangent   = normalize(cross(up, N));
    vec3 bitangent = cross(N, tangent);

    vec3 sampleVec = tangent * H.x + bitangent * H.y + N * H.z;
    return normalize(sampleVec);
}
vec3 PrefilterMap(vec3 N, float roughness)
{
    //vec3 N = normalize(localPos);
    vec3 R = N;
    vec3 V = R;

    const uint SAMPLE_COUNT = 1024u;
    float totalWeight = 0.0;
    vec3 prefilteredColor = vec3(0.0);
    for(uint i = 0u; i < SAMPLE_COUNT; ++i)
    {
        vec2 Xi = Hammersley(i, SAMPLE_COUNT);
        vec3 H  = ImportanceSampleGGX(Xi, N, roughness);
        vec3 L  = normalize(2.0 * dot(V, H) * H - V);

        float NdotL = max(dot(N, L), 0.0);
        if(NdotL > 0.0)
        {
            prefilteredColor += texture(samplerCube(environmentMap, environmentMapSampler), L).rgb * NdotL;
            totalWeight      += NdotL;
        }
    }
    prefilteredColor = prefilteredColor / totalWeight;
    return prefilteredColor;
}
float GeometrySchlickGGX_LUT(float NdotV, float roughness)
{
    float a = roughness;
    float k = (a * a) / 2.0;

    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}
float GeometrySmith_LUT(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX_LUT(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX_LUT(NdotL, roughness);

    return ggx1 * ggx2;
}
vec2 LUT(vec3 normal, vec3 viewDir, float roughness)
{
    float NdotV = max(dot(normal, viewDir), 0.0);

    vec3 V;
    V.x = sqrt(1.0 - NdotV*NdotV);
    V.y = 0.0;
    V.z = NdotV;

    float A = 0.0;
    float B = 0.0;

    vec3 N = vec3(0.0, 0.0, 1.0);

    const uint SAMPLE_COUNT = 1024u;
    for(uint i = 0u; i < SAMPLE_COUNT; ++i)
    {
        vec2 Xi = Hammersley(i, SAMPLE_COUNT);
        vec3 H  = ImportanceSampleGGX(Xi, N, roughness);
        vec3 L  = normalize(2.0 * dot(V, H) * H - V);

        float NdotL = max(L.z, 0.0);
        float NdotH = max(H.z, 0.0);
        float VdotH = max(dot(V, H), 0.0);

        if(NdotL > 0.0)
        {
            float G = GeometrySmith_LUT(N, V, L, roughness);
            float G_Vis = (G * VdotH) / (NdotH * NdotV);
            float Fc = pow(1.0 - VdotH, 5.0);

            A += (1.0 - Fc) * G_Vis;
            B += Fc * G_Vis;
        }
    }
    A /= float(SAMPLE_COUNT);
    B /= float(SAMPLE_COUNT);
    return vec2(A, B);
}
vec3 Enviroment(vec3 albedo, float metallic, float roughness, vec3 normal, vec3 viewDir, vec3 lightDir, vec2 uv)
{
    vec3 F0 = mix(vec3(0.04), albedo, metallic);
    vec3 F = fresnelSchlickRoughness(max(dot(normal, viewDir), 0.0), F0, roughness);
    
    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    vec3 irradiance = IrradianceMap(normal);
    vec3 diffuse    = irradiance * albedo;

    vec3 reflectDir = reflect(-viewDir, normal);
    vec3 prefilteredColor = PrefilterMap(normal, roughness);
    vec2 envBRDF  = LUT(normal, viewDir, roughness);
    vec3 specular = prefilteredColor * (F * envBRDF.x + envBRDF.y);
    
    vec3 ambient = (kD * diffuse + specular);

    return ambient;
}

// BSSRDF
//////////////////////////////////////////////////////////////////////////////////
vec3 Bssrdf(vec3 light, float thickness /*厚度*/, float distortion /*法线扰动*/, float powerScale, float scale, float attenuation /*消光系数*/, float ambient /*环境光*/, vec3 normal, vec3 lightDir, vec3 viewDir)
{
    vec3 H = normalize(lightDir + normal * distortion);
    float VdotH = pow(clamp(dot(viewDir, -H), 0.0, 1.0), powerScale) * scale;
    float translucency = attenuation * (VdotH + ambient) * thickness;
    return light * translucency;
}

void main() 
{
    vec4 shadowUV = light.projection * light.view * vec4(fragWorldPos, 1.0);
    shadowUV /= shadowUV.w;
    shadowUV.xy = shadowUV.xy * 0.5 + 0.5;

    float currentDepth = shadowUV.z;
    
    // Hard Shadow
    /*float shadowDepth = texture(sampler2D(shadowMap, shadowMapSampler), shadowCoord.xy).r;
    
    float bias = 0.01;
    // 同时这里注意要有=
    float shadow = currentDepth - bias >= shadowDepth ? 0.3 : 1.0;*/

    // PCF
    // float shadow = PCF(shadowCoord.xy, currentDepth);
    
    // 计算阴影要注意 ShadowMap值越大里光源越远, 这和很多文章不同, 他们是越大离得越近
    
    // PCSS
    float shadow = PCSS(shadowUV.xy, currentDepth);
    
    // 兰伯特光照
    /*vec3 normal = normalize(fragWorldNormal);
    float ndl = max(dot(normal, -light.dir), 0.0);
    vec3 albedo = vec3(1.0);
    vec3 diffuse = albedo * light.color.rgb * ndl * light.intensity;*/
    
    // PBR
    vec3 normal = normalize(fragWorldNormal);
    vec3 v = normalize(camera.worldPos.xyz - fragWorldPos);
    vec3 l = normalize(light.worldPos.xyz - fragWorldPos);
    vec3 albedo = vec3(1);
    float metallic = 0.9f;
    float roughness = 0.3f;
    vec3 pbr = PBR_GGX(albedo, metallic, roughness, normal, v, l, light.intensity * light.color.xyz);
    
    // Bssrdf
    //pbr += Bssrdf(,,,,,, normal, l, v);
    
    // IBL
    //vec3 environment = Enviroment(albedo, metallic, roughness, normal, v, l, uv.xy);

    //outColor.xyz = vec3(shadow);
    //outColor.xyz = pbr;
    //outColor.xyz = environment;
    outColor.xyz = pbr * shadow;
    //outColor.xyz = environment * pbr * shadow;

    outNormal = vec4(normal, 1.0);
    outPosition = vec4(fragWorldPos, 1.0);
    
    gl_FragDepth = currentDepth;
}