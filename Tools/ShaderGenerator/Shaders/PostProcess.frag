#version 450
#define PI 3.1415926536

layout(location = 0) in vec2 uv;
layout(location = 1) in vec3 viewDir;
layout(location = 0) out vec4 outColor;

layout(set = 0, binding = 0) uniform texture2D shadingResult;
layout(set = 0, binding = 1) uniform sampler shadingResultSampler;

layout(set = 0, binding = 2) uniform texture2D shadingDepthResult;
layout(set = 0, binding = 3) uniform sampler shadingDepthResultSampler;

layout(set = 0, binding = 4) uniform texture2D shadingNormalResult;
layout(set = 0, binding = 5) uniform sampler shadingNormalResultSampler;

layout(set = 0, binding = 6) uniform texture2D shadingPositionResult;
layout(set = 0, binding = 7) uniform sampler shadingPositionResultSampler;

layout (set = 0, binding = 8) uniform Camera
{
    mat4 view;
    mat4 projection;
    vec4 worldPos;
} camera;

layout (set = 0, binding = 9) uniform Light
{
    mat4 view;
    mat4 projection;
    vec3 dir;
    float intensity;
    vec4 color;
    vec4 worldPos;
} light;

/*layout (set = 0, binding = 10) uniform AtmosParams
{
    float planetRadius;     // 地表半径
    float topRadius;        // 大气层半径
    
    float rayleighScaleH;   // Rayleigh 标度高度
    float mieScaleH;        // Mie 标度高度

    vec3  betaRayleigh;     // Rayleigh 散射系数
    float padding;
    vec3  betaMie;       // Mie 散射系数

    float mieG;             // Mie 相函数异向性参数 g
} atmosParams;


// SkyMarching
//////////////////////////////////////////////////////////////////////////////////
const int cameraStepCount = 4;
const int lightStepCount = 4;

// 球与射线相交：返回进入和退出参数 tNear/tFar，若无相交则 tNear > tFar
// 射线 p + t*d，球心在原点，半径 r
bool intersectSphere(vec3 p, vec3 d, float r, out float tNear, out float tFar) 
{
    float b = dot(p, d);
    float c = dot(p, p) - r*r;
    float disc = b*b - c;
    if (disc < 0.0) { tNear = 1.0; tFar = 0.0; return false; }
    float s = sqrt(disc);
    tNear = -b - s;
    tFar  = -b + s;
    return tFar > 0.0;
}

// 海拔高度（离地表高度）
float altitude(vec3 p) 
{
    return abs(length(p) - atmosParams.planetRadius);
}

// 密度函数
float densityRayleigh(float h) { return max(exp(-h / atmosParams.rayleighScaleH), 0.0); }
float densityMie     (float h) { return max(exp(-h / atmosParams.mieScaleH),      0.0); }

// 相位函数
float phaseRayleigh(float cosTheta) 
{
    return 3.0 / (16.0 * PI) * (1.0 + cosTheta * cosTheta);
}
float phaseMie(float cosTheta) 
{
    float g2 = atmosParams.mieG * atmosParams.mieG;
    float denom = pow(1.0 + g2 - 2.0 * atmosParams.mieG * cosTheta, 1.5);
    float A = 3.0 * (1.0 - g2) / (8.0 * PI * (2.0 + g2));
    return A * (1.0 + cosTheta*cosTheta) / max(denom, 1e-6);
}

vec3 SkyMarching(vec3 viewDir)
{
    // 视线与大气层求交
    vec3 cameraPos = camera.worldPos.xyz;
    float tNear, tFar;
    // 如果没相交,说明相机在大气层外且没看向地球
    if (!intersectSphere(cameraPos, viewDir, atmosParams.topRadius, tNear, tFar))
    {
        return vec3(1);
    }
    float t0 = max(tNear, 0.0);
    float smax = tFar;

    vec3 result = vec3(0);
    
    // 从视线开始步进
    float ds_camera = (smax - t0) / cameraStepCount;
    vec3 tau_camera = vec3(0); // βr + βm
    for (int i = 0; i < cameraStepCount; ++i) 
    {
        vec3 p = cameraPos + viewDir * (t0 + (i + 0.5) * ds_camera);

        float h_camera = altitude(p);  // 海拔
        if (h_camera < 0.0) break;

        float rhoR_camera = densityRayleigh(h_camera); // ρr
        float rhoM_camera = densityMie(h_camera); // ρm
        
        // β = ∑(βr+βm)ds
        // 要考虑衰减,*密度函数
        tau_camera += (atmosParams.betaRayleigh * rhoR_camera + atmosParams.betaMie * rhoM_camera) * ds_camera;
        // T(camera, p)
        vec3 T_camera = exp(-tau_camera);
        
        vec3 tau_sun = vec3(0);
        float tGNear, tGFar;
        if(intersectSphere(p, -light.dir, atmosParams.planetRadius, tGNear, tGFar))
        {
            tau_sun = vec3(100.0);
        }
        else
        {
            float tSunNear, tSunFar;
            if (intersectSphere(p, -light.dir, atmosParams.topRadius, tSunNear, tSunFar))
            {
                float ds_sun = tSunFar  / float(lightStepCount);
                for (int j = 0; j < lightStepCount; ++j)
                {
                    vec3 q = p + (-light.dir) * (j + 0.5) * ds_sun;

                    float h_sun = altitude(q);
                    if (h_sun < 0.0)
                    {
                        tau_sun = vec3(100.0);
                        break;
                    }

                    float rhoR_sun = densityRayleigh(h_sun);
                    float rhoM_sun = densityMie(h_sun);

                    tau_sun += (atmosParams.betaRayleigh * rhoR_sun + atmosParams.betaMie * rhoM_sun) * ds_sun;
                }   
            }
        }
        // T(p, sun)
        vec3 T_sun = exp(-tau_sun);
        
        // S(p)
        float cosTheta = dot(viewDir, -light.dir);
        float Pr = phaseRayleigh(cosTheta);
        float Pm = phaseMie(cosTheta);
        vec3 S = atmosParams.betaRayleigh * rhoR_camera * Pr + atmosParams.betaMie * rhoM_camera * Pm;

        result += T_camera * S * T_sun * ds_camera;
    }
    result *= light.color.rgb * light.intensity;
    return result;
}*/

// SSAO
//////////////////////////////////////////////////////////////////////////////////
float Random(vec2 p) 
{
    return fract(sin(dot(p ,vec2(12.9898,78.233))) * 43758.5453);
}
vec3 Random(float i)
{
    float x = fract(sin(float(i) * 12.9898) * 43758.5453) * 2.0 - 1.0;
    float y = fract(sin(float(i) * 78.233) * 43758.5453) * 2.0 - 1.0;
    float z = fract(sin(float(i) * 53.313)) * 1.0; // [0,1]

    vec3 sampleDir = normalize(vec3(x, y, z));

    float scale = float(i) / 64.0;
    scale = mix(0.1, 1.0, scale * scale);
    return sampleDir * scale;
}

vec3 ToPosition(vec2 uv, float depth)
{
    float z = depth * 2.0 - 1.0;
    vec2 ndc = vec2(uv * 2.0 - 1.0);
    vec4 clip = vec4(ndc, z, 1.0);
    vec4 v = inverse(camera.projection) * clip; // view space * w
    return v.xyz / v.w;
}

float SSAO(vec2 uv, float radius)
{
    float viewDepth = texture(sampler2D(shadingDepthResult, shadingDepthResultSampler), uv).r;
    if (viewDepth >= 1.0) return 1.0;
    vec3 viewNormal = normalize(texture(sampler2D(shadingNormalResult, shadingNormalResultSampler), uv).rgb);
    vec3 viewPos = ToPosition(uv, viewDepth);

    vec3 randVec = normalize(vec3(Random(uv), Random(uv + 1.0), 0.0) * 2.0 - 1.0);;
    vec3 tangent = normalize(randVec - viewNormal * dot(randVec, viewNormal));
    vec3 bitangent = cross(viewNormal, tangent);
    mat3x3 TBN = mat3x3(tangent, bitangent, viewNormal);

    float occlusion = 0.0;
    const uint SAMPLE_COUNT = 32;
    for (int i = 0; i < SAMPLE_COUNT; ++i)
    {
        // 采样点位置
        vec3 viewSamplePos = viewPos + (TBN * (Random(i) * radius)).xyz;
        vec4 clipSamplePos = camera.projection * vec4(viewSamplePos, 1.0);
        clipSamplePos /= clipSamplePos.w;
        
        // 采样点UV
        vec2 sampleUV = clipSamplePos.xy * 0.5 + 0.5;
        if (sampleUV.x < 0.0 || sampleUV.x > 1.0 || sampleUV.y < 0.0 || sampleUV.y > 1.0)
                continue;
        
        float nearSampleDepth = texture(sampler2D(shadingDepthResult, shadingDepthResultSampler), sampleUV).r;
        if (nearSampleDepth >= 1.0) continue;
        
        // 采样点UV对应的物体的位置
        vec3 viewNearSamplePos = ToPosition(sampleUV, nearSampleDepth);
        
        float rangeCheck = smoothstep(0.0, 1.0, radius / abs(viewPos.z - viewNearSamplePos.z));
        float occ = viewNearSamplePos.z > viewPos.z + 0.01 ? 1.0 : 0.0;
        occlusion += occ * rangeCheck;
    }

    occlusion = 1.0 - (occlusion / float(SAMPLE_COUNT));
    return clamp(occlusion, 0.0, 1.0);
}

void main()
{
    float depth = texture(sampler2D(shadingDepthResult, shadingDepthResultSampler), uv).r;
    vec3 color = texture(sampler2D(shadingResult, shadingResultSampler), uv).rgb;
    vec3 normal = texture(sampler2D(shadingNormalResult, shadingNormalResultSampler), uv).rgb;
    vec3 position = texture(sampler2D(shadingPositionResult, shadingPositionResultSampler), uv).rgb;
    
    if (depth >= 1.0)outColor = vec4(vec3(0.1), 1);
    else outColor = vec4(color * SSAO(uv, 1), 1);
    
    //outColor.rgb = SkyMarching(viewDir);
}