using System.Numerics;
using System.Runtime.InteropServices;

namespace ET.Client;

[StructLayout(LayoutKind.Sequential)]
public partial struct PerlinNoise_InputBuffer
{
    public Vector4 area;
    public Vector4 cellSize;
    public Vector4 center;

    public Vector4 diff;
    public float mapCount;
    public float loud;
    public float roughness;
    public float persistence;
}

[StructLayout(LayoutKind.Sequential)]
public partial struct PerlinNoise_OutputBuffer
{
    public Vector4[] vertices;
}

[StructLayout(LayoutKind.Sequential)]
public partial struct MarchingCube_InputBuffer
{
    public Vector3 area;
    public float maxValue;
    public Vector4[] vertices;
}

// 查Ai说不能把Triangle[]放里面
[StructLayout(LayoutKind.Sequential)]
public partial struct MarchingCube_OutputBuffer
{
    public uint count;
    private Vector3 pad;
}

[StructLayout(LayoutKind.Sequential)]
struct Triangle
{
    public Vector3 A;
    private float padA;
    public Vector3 B;
    private float padB;
    public Vector3 C;
    private float padC;
}

public partial struct Atmosphere_InputBuffer
{
    public int TRANSMITTANCE_TEXTURE_WIDTH;
    public int TRANSMITTANCE_TEXTURE_HEIGHT;
    public int SCATTERING_TEXTURE_R_SIZE;
    public int SCATTERING_TEXTURE_MU_SIZE;
    public int SCATTERING_TEXTURE_MU_S_SIZE;
    public int SCATTERING_TEXTURE_NU_SIZE;
    public int SCATTERING_TEXTURE_WIDTH;
    public int SCATTERING_TEXTURE_HEIGHT;
    public int SCATTERING_TEXTURE_DEPTH;
    public int IRRADIANCE_TEXTURE_WIDTH;
    public int IRRADIANCE_TEXTURE_HEIGHT;
    public int padding0;

    public Vector3 SKY_SPECTRAL_RADIANCE_TO_LUMINANCE;
    private int padding1;
    public Vector3 SUN_SPECTRAL_RADIANCE_TO_LUMINANCE;
    private int padding2;

    public Matrix4x4 luminanceFromRadiance;

    public float rayleigh_width;
    public float rayleigh_exp_term;
    public float rayleigh_exp_scale;
    public float rayleigh_linear_term;
    public float rayleigh_constant_term;

    public Vector3 rayleigh_scattering;
    
    public float mie_width;
    public float mie_exp_term;
    public float mie_exp_scale;
    public float mie_linear_term;
    public float mie_constant_term;
    
    public Vector3 mie_scattering;
    
    public float absorption0_width;
    public float absorption0_exp_term;
    public float absorption0_exp_scale;
    public float absorption0_linear_term;
    public float absorption0_constant_term;
    
    public float absorption1_width;
    public float absorption1_exp_term;
    public float absorption1_exp_scale;
    public float absorption1_linear_term;
    public float absorption1_constant_term;
    
    private int padding3;
    private int padding4;
    
    public Vector3 absorption_extinction;
    private int padding5;
    
    public Vector3 solar_irradiance;
    private int padding6;

    public float sun_angular_radius;
    public float bottom_radius;
    public float top_radius;
    private int padding7;

    public Vector3 mie_extinction;
    public float mie_phase_function_g;

    public Vector3 ground_albedo;
    
    public float mu_s_min;
}