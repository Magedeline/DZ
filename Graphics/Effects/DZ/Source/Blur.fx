#include "Common.fxh"

//-----------------------------------------------------------------------------
// Simple Blur Shader
// A lightweight separable box blur for post-process softening.
//-----------------------------------------------------------------------------

DECLARE_TEXTURE(text, 0);

uniform float2 pixel;
uniform float strength = 1.0;

//-----------------------------------------------------------------------------
// Pixel Shaders.
//-----------------------------------------------------------------------------

float4 PS_Blur5(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : SV_TARGET0
{
    float4 color = 0;
    float2 offset = pixel * strength;

    color += SAMPLE_TEXTURE(text, uv - offset * 2.0) * 0.10;
    color += SAMPLE_TEXTURE(text, uv - offset)         * 0.20;
    color += SAMPLE_TEXTURE(text, uv)                * 0.40;
    color += SAMPLE_TEXTURE(text, uv + offset)         * 0.20;
    color += SAMPLE_TEXTURE(text, uv + offset * 2.0) * 0.10;

    return color * inColor;
}

float4 PS_Blur3(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : SV_TARGET0
{
    float4 color = 0;
    float2 offset = pixel * strength;

    color += SAMPLE_TEXTURE(text, uv - offset) * 0.25;
    color += SAMPLE_TEXTURE(text, uv)          * 0.50;
    color += SAMPLE_TEXTURE(text, uv + offset) * 0.25;

    return color * inColor;
}

//-----------------------------------------------------------------------------
// Techniques.
//-----------------------------------------------------------------------------

technique Blur5
{
    pass
    {
        PixelShader = compile PS_2_SHADER_COMPILER PS_Blur5();
    }
}

technique Blur3
{
    pass
    {
        PixelShader = compile PS_2_SHADER_COMPILER PS_Blur3();
    }
}
