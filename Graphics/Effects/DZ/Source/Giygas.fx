#include "Common.fxh"

//-----------------------------------------------------------------------------
// Giygas HLSL Shader
// Adapted from GDI+ ColorMatrix and LockBits pixel distortion concepts.
// Fits within ps_2_0 (64 arithmetic instruction limit).
//-----------------------------------------------------------------------------

DECLARE_TEXTURE(text, 0);

uniform float time;
uniform float intensity = 1.0;
uniform float distortionStrength = 1.0;
uniform float2 screenSize = float2(320.0, 180.0);

//-----------------------------------------------------------------------------
// Pixel Shaders.
//-----------------------------------------------------------------------------

// Full effect: UV wave distortion + chromatic aberration + psychedelic color shift
float4 PS_Giygas(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : SV_TARGET0
{
    // GDI+ style UV wave distortion (inline to save instructions)
    float waveU = sin(uv.y * screenSize.y * 0.05 + time * 2.0) * 0.03 * intensity * distortionStrength;
    float2 distortedUV = float2(uv.x + waveU, uv.y);

    // Chromatic aberration: 2 texture samples (RG from main, B from offset)
    float chromatic = 0.015 * intensity * distortionStrength;
    float4 color = SAMPLE_TEXTURE(text, distortedUV);
    float blueSample = SAMPLE_TEXTURE(text, distortedUV + float2(-chromatic, 0.0)).b;
    color.b = blueSample;

    // GDI+ style ColorMatrix psychedelic color shift
    float hueShift = time + uv.x * 5.0 + uv.y * 3.0;
    color.r = saturate(color.r + sin(hueShift * 1.5) * 0.12 + 0.08);
    color.g = saturate(color.g + cos(hueShift * 1.2) * 0.10 + 0.06);
    color.b = saturate(color.b + sin(hueShift * 0.9) * 0.08 + 0.04);

    return color * inColor;
}

// Distortion only (UV wave)
float4 PS_GiygasDistort(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : SV_TARGET0
{
    float waveU = sin(uv.y * screenSize.y * 0.05 + time * 2.0) * 0.03 * intensity * distortionStrength;
    float2 distortedUV = float2(uv.x + waveU, uv.y);
    return SAMPLE_TEXTURE(text, distortedUV) * inColor;
}

// Psychedelic color shift only
float4 PS_GiygasColor(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : SV_TARGET0
{
    float4 color = SAMPLE_TEXTURE(text, uv);
    float hueShift = time + uv.x * 5.0 + uv.y * 3.0;
    color.r = saturate(color.r + sin(hueShift * 1.5) * 0.12 + 0.08);
    color.g = saturate(color.g + cos(hueShift * 1.2) * 0.10 + 0.06);
    color.b = saturate(color.b + sin(hueShift * 0.9) * 0.08 + 0.04);
    return color * inColor;
}

// Chromatic aberration only
float4 PS_GiygasChromatic(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : SV_TARGET0
{
    float chromatic = 0.015 * intensity * distortionStrength;
    float4 color = SAMPLE_TEXTURE(text, uv);
    color.b = SAMPLE_TEXTURE(text, uv + float2(-chromatic, 0.0)).b;
    return color * inColor;
}

//-----------------------------------------------------------------------------
// Techniques.
//-----------------------------------------------------------------------------

technique Giygas
{
    pass
    {
        PixelShader = compile PS_2_SHADER_COMPILER PS_Giygas();
    }
}

technique GiygasDistort
{
    pass
    {
        PixelShader = compile PS_2_SHADER_COMPILER PS_GiygasDistort();
    }
}

technique GiygasColor
{
    pass
    {
        PixelShader = compile PS_2_SHADER_COMPILER PS_GiygasColor();
    }
}

technique GiygasChromatic
{
    pass
    {
        PixelShader = compile PS_2_SHADER_COMPILER PS_GiygasChromatic();
    }
}
