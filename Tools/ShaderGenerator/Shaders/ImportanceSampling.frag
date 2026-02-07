#version 450

layout(location = 0) out vec4 outColor;

vec3 LightSample()
{
    return vec3(1.0);
}

float LightPDF()
{
    return 0;
}

vec3 BrdfSample()
{
    return vec3(1.0);
}

float BrdfPDF()
{
    return 0;
}

void main()
{
    float brdfPDF = BrdfPDF();
    float lightPDF = LightPDF();
    vec3 brdfSample = BrdfSample();
    vec3 lightSample = LightSample();
    
    float w_B = (brdfPDF * brdfPDF) / (brdfPDF * brdfPDF + lightPDF * lightPDF);
    brdfSample *= w_B;
    
    float w_L = (lightPDF * lightPDF) / (lightPDF * lightPDF + brdfPDF * brdfPDF);
    lightSample *= w_L;

    outColor.xyz = brdfSample + lightSample;
}