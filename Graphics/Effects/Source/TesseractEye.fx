#include "Common.fxh"

//-----------------------------------------------------------------------------
// Tesseract Eye Shader
// A rotating geometric eye with tesseract/hypercube-inspired wireframe rings.
// Features color-cycling geometric patterns around a central eye pupil.
//-----------------------------------------------------------------------------

DECLARE_TEXTURE(text, 0);

uniform float time;
uniform float intensity = 1.0;
uniform float2 screenSize = float2(320.0, 180.0);

//-----------------------------------------------------------------------------
// Pixel Shaders.
//-----------------------------------------------------------------------------

float4 PS_TesseractEye(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : SV_TARGET0
{
    float2 center = float2(0.5, 0.5);
    float dist = length(uv - center);
    float4 color = SAMPLE_TEXTURE(text, uv);

    // --- Central Pupil ---
    float pupil = step(dist, 0.06);
    float iris = step(dist, 0.16) - pupil;
    float t = time * 0.3;
    float3 irisColor = float3(sin(t) * 0.5 + 0.5, sin(t + 2.1) * 0.5 + 0.5, sin(t + 4.2) * 0.5 + 0.5);
    color.rgb += irisColor * iris * 0.4 * intensity;
    color.rgb += float3(0.05, 0.05, 0.05) * pupil * intensity;

    // --- Distance-based geometric rings (no atan2) ---
    float ringWave = sin(dist * 30.0 - time * 2.0) * 0.5 + 0.5;
    float ringA = step(abs(dist - 0.32), 0.015) * ringWave;
    float ringB = step(abs(dist - 0.44), 0.012) * (1.0 - ringWave);
    color.rgb += float3(0.7, 0.2, 0.9) * ringA * intensity;
    color.rgb += float3(0.2, 0.7, 1.0) * ringB * intensity;

    // --- Simple cross-hair connectors ---
    float cx = abs(uv.x - 0.5);
    float cy = abs(uv.y - 0.5);
    float cross = step(cx, 0.005) * step(cy, 0.25);
    cross += step(cy, 0.005) * step(cx, 0.25);
    color.rgb += float3(0.3, 0.9, 0.5) * cross * 0.3 * intensity;

    return color * inColor;
}

// Eye only (no rings)
float4 PS_TesseractEye_EyeOnly(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : SV_TARGET0
{
    float2 center = float2(0.5, 0.5);
    float dist = length(uv - center);
    float4 color = SAMPLE_TEXTURE(text, uv);
    float pupil = step(dist, 0.06);
    float iris = step(dist, 0.16) - pupil;
    float t = time * 0.3;
    float3 irisColor = float3(sin(t) * 0.5 + 0.5, sin(t + 2.1) * 0.5 + 0.5, sin(t + 4.2) * 0.5 + 0.5);
    color.rgb += irisColor * iris * 0.4 * intensity;
    color.rgb += float3(0.05, 0.05, 0.05) * pupil * intensity;
    return color * inColor;
}

//-----------------------------------------------------------------------------
// Techniques.
//-----------------------------------------------------------------------------

technique TesseractEye
{
    pass
    {
        PixelShader = compile PS_2_SHADER_COMPILER PS_TesseractEye();
    }
}

technique TesseractEyeEyeOnly
{
    pass
    {
        PixelShader = compile PS_2_SHADER_COMPILER PS_TesseractEye_EyeOnly();
    }
}
