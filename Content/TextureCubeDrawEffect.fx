
// https://www.geeks3d.com/20141201/how-to-rotate-a-vertex-by-a-quaternion-in-glsl/


#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

matrix World;
matrix View;
matrix Projection;
float3 CameraPosition;
int testValue1;


TextureCube CubeMap;
samplerCUBE CubeMapSampler = sampler_state
{
    texture = <CubeMap>;
    //magfilter = Linear;
    //minfilter = Linear;
    //mipfilter = Linear;
    AddressU = clamp;
    AddressV = clamp;
};

//____________________________________
// structs
//____________________________________


struct RenderCubeVertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexureCoordinate : TEXCOORD0;
};

struct RenderCubeVertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Position3D : TEXCOORD1;
    float3 Normal3D : TEXCOORD2;
    float2 TexureCoordinate : TEXCOORD0;
};



//____________________________________
// shaders and technique  RenderCubeMap
//____________________________________

RenderCubeVertexShaderOutput RenderCubeMapVS(in RenderCubeVertexShaderInput input)
{
    RenderCubeVertexShaderOutput output;
    float4x4 vp = mul(View, Projection);
    float4 pos = mul(input.Position, World);
    float4 norm = mul(input.Normal, World);
    output.Position = mul(pos, vp);
    output.Position3D = mul(pos.xyz, View);
    output.Normal3D = norm.xyz;
    output.TexureCoordinate = input.TexureCoordinate;
    return output;
}

float4 RenderCubeMapPS(RenderCubeVertexShaderOutput input) : COLOR
{
    //float4 baseColor = tex2D(TextureSamplerDiffuse, input.TexureCoordinate); 
    ////clip(baseColor.a - .01f); // just straight clip super low alpha.
    //float3 P = input.Position3D;
    //float3 N = normalize(input.Normal3D.xyz);
    //float3 V = normalize(CameraPosition - input.Position3D);
    //float NdotV = max(0.0, dot(N, V));
    //float3 R = 2.0 * NdotV * N - V;

    //float4 envMapColor = texCUBElod(CubeMapSampler, float4(R, testValue1));
    //return float4(envMapColor.rgb, 1.0f);

    float3 N = normalize(input.Normal3D.xyz);
    float4 envMapColor = texCUBElod(CubeMapSampler, float4(N, testValue1));
    return float4(envMapColor.rgb, 1.0f);
}

technique RenderCubeMap
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL
            RenderCubeMapVS();
        PixelShader = compile PS_SHADERMODEL
            RenderCubeMapPS();
    }
};