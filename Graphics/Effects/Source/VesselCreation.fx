#include "Common.fxh"

//-----------------------------------------------------------------------------
// Vessel Creation Shader
// Dark, mysterious void atmosphere inspired by Deltarune/Hollow Knight
// vessel creation sequences. Subtle soul glow and depth fog.
//-----------------------------------------------------------------------------

DECLARE_TEXTURE(text, 0);

uniform float time;
uniform float intensity = 1.0;
uniform float2 screenSize = float2(320.0, 180.0);

//-----------------------------------------------------------------------------
// Pixel Shaders.
//-----------------------------------------------------------------------------

float4 PS_VesselCreation(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : SV_TARGET0
{
    float2 center = float2(0.5, 0.5);
    float2 delta = uv - center;
    float dist = length(delta);

    // Sample the scene
    float4 color = SAMPLE_TEXTURE(text, uv);

    // Radial darkening (depth fog / void atmosphere)
    float fog = 1.0 - smoothstep(0.1, 0.7, dist);
    color.rgb *= lerp(0.15, 1.0, fog);

    // Central soul glow (pulsing white-blue light)
    float pulse = sin(time * 1.5) * 0.15 + 0.85;
    float glow = exp(-dist * 8.0) * pulse * intensity;
    float3 glowColor = float3(0.85, 0.9, 1.0) * glow * 0.4;
    color.rgb += glowColor;

    // Subtle scanline flicker
    float scanline = sin(uv.y * screenSize.y * 0.5 + time * 0.3) * 0.03 + 0.97;
    color.rgb *= scanline;

    return color * inColor;
}

float4 PS_VesselCreationVoidOnly(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : SV_TARGET0
{
    float2 center = float2(0.5, 0.5);
    float dist = length(uv - center);
    float4 color = SAMPLE_TEXTURE(text, uv);
    float fog = 1.0 - smoothstep(0.1, 0.7, dist);
    color.rgb *= lerp(0.15, 1.0, fog);
    return color * inColor;
}

float4 PS_VesselCreationSoulGlow(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : SV_TARGET0
{
    float2 center = float2(0.5, 0.5);
    float dist = length(uv - center);
    float4 color = SAMPLE_TEXTURE(text, uv);
    float pulse = sin(time * 1.5) * 0.15 + 0.85;
    float glow = exp(-dist * 8.0) * pulse * intensity;
    color.rgb += float3(0.85, 0.9, 1.0) * glow * 0.4;
    return color * inColor;
}

//-----------------------------------------------------------------------------
// Techniques.
//-----------------------------------------------------------------------------

technique VesselCreation
{
    pass
    {
        PixelShader = compile PS_2_SHADER_COMPILER PS_VesselCreation();
    }
}

technique VesselCreationVoidOnly
{
    pass
    {
        PixelShader = compile PS_2_SHADER_COMPILER PS_VesselCreationVoidOnly();
    }
}

technique VesselCreationSoulGlow
{
    pass
    {
        PixelShader = compile PS_2_SHADER_COMPILER PS_VesselCreationSoulGlow();
    }
}
