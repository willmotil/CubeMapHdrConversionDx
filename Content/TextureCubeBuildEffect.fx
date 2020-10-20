﻿


// https://www.geeks3d.com/20141201/how-to-rotate-a-vertex-by-a-quaternion-in-glsl/


#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif
#define PI 3.14159265359f
#define ToDegrees 57.295779513f;
#define ToRadians 0.0174532925f;

int FaceToMap;

Texture2D Texture; // primary texture.
sampler2D TextureSamplerDiffuse = sampler_state
{
    texture = <Texture>;
};

TextureCube CubeMap;
samplerCUBE CubeMapSampler = sampler_state
{
    texture = <CubeMap>;
    magfilter = Linear;
    minfilter = Linear;
    mipfilter = Linear;
    AddressU = clamp;
    AddressV = clamp;
};


//____________________________________
// structs
//____________________________________

struct HdrToCubeMapVertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexureCoordinate : TEXCOORD0;
};

struct HdrToCubeMapVertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Position3D : TEXCOORD1;
    float2 TexureCoordinate : TEXCOORD0;
};

struct FaceStruct
{
    float3 PositionNormal;
    float3 FaceNormal;
    float3 FaceUp;
};


//____________________________________
// functions
//____________________________________

// I made this up to do this tranform because i couldn't find the code to do it anywere.
// Ok so what people in a couple examples are doing regularly is like a matrix view transform, im not sure that is actually any better 
// since i do have to calculate the extrude the normal from the piexel anyways but i should test it later down the road.

FaceStruct UvFaceToCubeMapVector(float2 pos, int faceIndex)
{
    FaceStruct output = (FaceStruct)0;
    float u = pos.x;
    float v = pos.y;
    switch (abs(faceIndex))
    {
    case 1: //FACE_LEFT: CubeMapFace.NegativeX
        output.PositionNormal = float3(-1.0f, v, u);
        output.FaceNormal = float3(-1.0f, 0, 0);
        output.FaceUp = float3(0, 1, 0);
        break;
    case 5: // FACE_FORWARD: CubeMapFace.NegativeZ
        output.PositionNormal = float3(-u, v, -1.0f);
        output.FaceNormal = float3(0, 0, -1.0f);
        output.FaceUp = float3(0, 1, 0);
        break;
    case 0: //FACE_RIGHT: CubeMapFace.PositiveX
        output.PositionNormal = float3(1.0f, v, -u);
        output.FaceNormal = float3(1.0f, 0, 0);
        output.FaceUp = float3(0, 1, 0);
        break;
    case 4: //FACE_BACK: CubeMapFace.PositiveZ
        output.PositionNormal = float3(u, v, 1.0f);
        output.FaceNormal = float3(0, 0, 1.0f);
        output.FaceUp = float3(0, 1, 0);
        break;

    case 2: //FACE_TOP: CubeMapFace.PositiveY
        output.PositionNormal = float3(u, 1.0f, -v);
        output.FaceNormal = float3(0, 1.0f, 0);
        output.FaceUp = float3(0, 0, 1);
        break;
    case 3: //FACE_BOTTOM : CubeMapFace.NegativeY
        output.PositionNormal = float3(u, -1.0f, v);   // dir = float3(v, -1.0f, u);
        output.FaceNormal = float3(0, -1.0f, 0);
        output.FaceUp = float3(0, 0, -1);
        break;

    default:
        output.PositionNormal = float3(-1.0f, v, u); // na
        output.FaceNormal = float3(-1.0f, 0, 0);
        output.FaceUp = float3(0, 1, 0);
        break;
    }
    //output.PositionNormal = new Vector3(output.PositionNormal.z, -output.PositionNormal.y, output.PositionNormal.x); // invert
    output.PositionNormal = normalize(output.PositionNormal);
    return output;
}

float2 CubeMapNormalTo2dEquaRectangularMapUvCoordinates(float3 normal)
{
    float2 uv = float2((float)atan2(-normal.z, normal.x), (float)asin(normal.y));
    float2 INVERT_ATAN = float2(0.1591f, 0.3183f);
    uv = uv * INVERT_ATAN + float2(0.5f, 0.5f);
    return uv;
}

float2 CubeMapNormalTo2dEquaRectangularMapUvCoordinatesAlt(float3 a_coords)
{
    float pi = 3.141592653589793f;
    float3 a_coords_n = normalize(a_coords);
    float lon = atan2(a_coords_n.z, a_coords_n.x);
    float lat = acos(a_coords_n.y);
    float2 sphereCoords = float2(lon, lat) * (1.0f / pi);
    return float2(sphereCoords.x * 0.5f + 0.5f, 1.0f - sphereCoords.y);
}

float3 EquaRectangularMapUvCoordinatesTo3dCubeMapNormal(float2 uvCoords)
{
    float pi = 3.14159265358f;
    float3 v = float3(0.0f, 0.0f, 0.0f);
    float2 uv = uvCoords;
    uv *= float2(2.0f * pi, pi);
    float siny = sin(uv.y);
    v.x = -sin(uv.x) * siny;
    v.y = cos(uv.y);
    v.z = -cos(uv.x) * siny;
    //v = new Vector3(v.Z, -v.Y, v.X);
    return v;
}

// http://www.codinglabs.net/article_physically_based_rendering.aspx
//
// F  needs work.
//
float4 GetIrradiance(float2 pixelpos, int faceToMap)
{
    FaceStruct input = UvFaceToCubeMapVector(pixelpos, faceToMap);

    float3 normal = input.PositionNormal;
    float3 up = input.FaceUp;
    float3 right = normalize(cross(up, input.FaceNormal));
    up = cross(input.FaceNormal, right);

    // the following values are in degrees
    float numberOfSamplesHemisphere = 15; // we want the smallest amount with good quality
    float numberOfSamplesAround = 30; // same as above
    float hemisphereMaxAngle = 45.0f; // we really want 90

    float minimumAdjustment = 2.1f; // this is to help control the sampling geometry.
    float mipSampleLevel = 0; // this is the sample or mipmap level from the enviromental map we take the current pixel from.

    // calculated from the above for adjusting the loop geometry pattern.
    float hemisphereMaxAngleTheta = hemisphereMaxAngle * ToRadians; // computed
    float stepTheta = (hemisphereMaxAngle / numberOfSamplesHemisphere) * ToRadians - 0.05f;
    float stepPhi = (360.0f / numberOfSamplesAround) * ToRadians - 0.05f;

    float3 accumulatedColor = float3(0, 0, 0);
    float totalWeight = 0;
    float3 averagedColor = float3(0, 0, 0);
    float totalSampleCount = 0;
    
    // sample enviromental cubemap
    for (float phi = 0.01; phi < 6.283; phi += stepPhi) // z rot.
    {
        for (float theta = 0.01f; theta < hemisphereMaxAngleTheta; theta += stepTheta) // y 
        {
            // calculate the new vector around the normal to sample rotationally.
            float3 temp = cos(phi) * right + sin(phi) * up;
            float4 sampleVector = float4(cos(theta) * normal + sin(theta) * temp, mipSampleLevel);
            float3 sampledColor = texCUBElod(CubeMapSampler, sampleVector).rgb;

            // some possible weighting functions.
            
            float avg = (sampledColor.r + sampledColor.b + sampledColor.g) * 0.33333f;
            float NdotS = saturate(dot(normal, sampleVector.rgb));
            float phiMuliplier = 1.0f - (phi / (5.283f + 1.0f));

            // accumulate and weigh the geometrical sampled pattern ... here is the hard part.

            //accumulatedColor += sampledColor;
            //totalWeight++;

            //accumulatedColor += sampledColor * NdotS;
            //totalWeight += NdotS;

            //accumulatedColor += sampledColor * (cos(theta) * sin(theta));
            //totalWeight += cos(theta) * sin(theta);

            accumulatedColor += sampledColor * phiMuliplier;
            totalWeight += phiMuliplier;
        }

    }
    //float4 final = float4(PI * accumulatedColor / totalWeight, 1.0f );
    float4 final = float4(accumulatedColor / totalWeight, 1.0f);

    // overlaid visualization.
    //float3 directColor = texCUBElod(CubeMapSampler, float4(normal, 0)).rgb;
    //directColor.rgb = (directColor.r + directColor.g + directColor.b) / 3;
    //final.rgb = final.rgb * 0.90f + directColor.brg * 0.10f;
    return final;
}








//____________________________________
// shaders and technique HdrToEnvCubeMap
// Copy 2d spherical hdr to enviromental cubemap
//____________________________________

HdrToCubeMapVertexShaderOutput HdrToEnvCubeMapVS(in HdrToCubeMapVertexShaderInput input)
{
    HdrToCubeMapVertexShaderOutput output = (HdrToCubeMapVertexShaderOutput)0;
    output.Position = input.Position;
    output.Position3D = input.Position;
    return output;
}

float4 HdrToEnvCubeMapPS(HdrToCubeMapVertexShaderOutput input) : COLOR
{
    FaceStruct face = UvFaceToCubeMapVector(input.Position3D, FaceToMap);
    float3 v = face.PositionNormal;
    float2 uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinates(v);
    float2 texcoords = float2(uv.x, 1.0f - uv.y);  // raw dx transform
    float4 color = float4(tex2D(TextureSamplerDiffuse, texcoords).rgb, 1.0f);
    return color;
}

technique HdrToEnvCubeMap
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL
            HdrToEnvCubeMapVS();
        PixelShader = compile PS_SHADERMODEL
            HdrToEnvCubeMapPS();
    }
};


//____________________________________
// shaders and technique CubeMapToEnvHdr
// Copy enviromental cubemap to 2d spherical
//____________________________________

float4 CubeMapToTexturePS(HdrToCubeMapVertexShaderOutput input) : COLOR
{
    float3 v = EquaRectangularMapUvCoordinatesTo3dCubeMapNormal(input.Position3D.xy);
    return texCUBElod(CubeMapSampler, float4(v, 0.0f) );
}

technique CubeMapToEnvHdr
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL
            HdrToEnvCubeMapVS();
        PixelShader = compile PS_SHADERMODEL
            CubeMapToTexturePS();
    }
};

//____________________________________
// shaders and technique FaceToFace
// Copy enviromental cubemap to 2d spherical
//____________________________________


float4 FaceToFacePS(HdrToCubeMapVertexShaderOutput input) : COLOR
{
    //float2 texcoords = float2(uv.x, 1.0f - uv.y);  // raw dx transform
    float4 color = float4(tex2D(TextureSamplerDiffuse, input.Position3D.xy).rgb, 1.0f);
    return color;
}

technique CubeMapToEnvHdr
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL
            HdrToEnvCubeMapVS();
        PixelShader = compile PS_SHADERMODEL
            FaceToFacePS();
    }
};

//____________________________________
// shaders and technique EnvCubemapToDiffuseIlluminationCubeMap
// Generate diffuse illumination map from enviroment cubemap.
//____________________________________

HdrToCubeMapVertexShaderOutput HdrToDiffuseIlluminationCubeMapVS(in HdrToCubeMapVertexShaderInput input)
{
    HdrToCubeMapVertexShaderOutput output = (HdrToCubeMapVertexShaderOutput)0;
    output.Position = input.Position;
    output.Position3D = input.Position;
    return output;
}

float4 HdrToDiffuseIlluminationCubeMapPS(HdrToCubeMapVertexShaderOutput input) : COLOR
{
    return GetIrradiance(input.Position3D, FaceToMap);
}

technique EnvCubemapToDiffuseIlluminationCubeMap
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL
            HdrToDiffuseIlluminationCubeMapVS();
        PixelShader = compile PS_SHADERMODEL
            HdrToDiffuseIlluminationCubeMapPS();
    }
};










//float2 CubeMapVectorToUvFace(float3 v, out int faceIndex)
//{
//    float3 vAbs = Abs(v);
//    float ma;
//    float2 uv;
//    if (vAbs.z >= vAbs.x && vAbs.z >= vAbs.y)
//    {
//        faceIndex = v.z < 0.0 ? FACE_FRONT : FACE_BACK;   // z major axis.  we designate negative z forward.
//        ma = 0.5f / vAbs.z;
//        uv = float2(v.z < 0.0f ? -v.x : v.x, -v.y);
//    }
//    else if (vAbs.y >= vAbs.x)
//    {
//        faceIndex = v.y < 0.0f ? FACE_BOTTOM : FACE_TOP;  // y major axis.
//        ma = 0.5f / vAbs.y;
//        uv = float2(v.x, v.y < 0.0 ? -v.z : v.z);
//    }
//    else
//    {
//        faceIndex = v.X < 0.0 ? FACE_LEFT : FACE_RIGHT; // x major axis.
//        ma = 0.5f / vAbs.X;
//        uv = float2(v.X < 0.0 ? v.Z : -v.Z, -v.Y);
//    }
//    return uv * ma + float2(0.5f, 0.5f);
//}



//// render cube maps face
//float4 CubeToFaceCopy(float3 pixelpos, int face)
//{
//    FaceStruct input = UvFaceToCubeMapVector(pixelpos, face);
//    return  float4(texCUBElod(CubeMapSampler, float4(input.PositionNormal, 0.0f)).rgb, 1.0f);
//}

//// render texture2d regular.
//float4 Face2DToFaceCopy(float3 pixelpos, int face)
//{
//    return  float4(tex2D(TextureSamplerDiffuse, pixelpos.xy).rgb, 1.0f);
//}
