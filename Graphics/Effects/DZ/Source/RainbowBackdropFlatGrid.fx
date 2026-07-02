#include "Common.fxh"

//-----------------------------------------------------------------------------
// Rainbow Backdrop Flat Grid Shader
// A scrolling synthwave/retrowave style flat grid with rainbow-colored lines.
// Grid scrolls toward the viewer with perspective fade.
//-----------------------------------------------------------------------------

DECLARE_TEXTURE(text, 0);

uniform float time;
uniform float speed = 1.0;
uniform float intensity = 1.0;
uniform float2 screenSize = float2(320.0, 180.0);

//-----------------------------------------------------------------------------
// Pixel Shaders.
//-----------------------------------------------------------------------------

float4 PS_RainbowBackdropFlatGrid(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : SV_TARGET0
{
    float4 color = SAMPLE_TEXTURE(text, uv);

    // Horizon at 60% down the screen
    float horizonY = 0.55;
    float belowHorizon = saturate((uv.y - horizonY) / (1.0 - horizonY));

    // Perspective depth factor
    float depth = belowHorizon * belowHorizon;

    // Scrolling grid offset
    float scroll = time * speed * 0.5;

    // Horizontal grid lines (perspective spaced)
    float lineY = frac(log(belowHorizon + 0.01) * 3.0 + scroll);
    float hLine = smoothstep(0.08, 0.0, abs(lineY - 0.5)) * belowHorizon;

    // Vertical grid lines (fan out from center)
    float centerX = (uv.x - 0.5) / (belowHorizon + 0.05);
    float lineX = frac(centerX + scroll * 0.3);
    float vLine = smoothstep(0.06, 0.0, abs(lineX - 0.5)) * belowHorizon;

    float grid = saturate(hLine + vLine);

    // Rainbow color for grid lines
    float hue = (time * 0.2 + uv.x * 0.3 + belowHorizon * 0.5) % 1.0;
    float3 rainbow;
    rainbow.r = abs(hue * 6.0 - 3.0) - 1.0;
    rainbow.g = 2.0 - abs(hue * 6.0 - 2.0);
    rainbow.b = 2.0 - abs(hue * 6.0 - 4.0);
    rainbow = saturate(rainbow);

    // Fade grid near horizon and screen edges
    float edgeFade = 1.0 - abs(uv.x - 0.5) * 1.5;
    edgeFade = saturate(edgeFade);

    color.rgb += rainbow * grid * edgeFade * depth * 0.6 * intensity;
    color.a = saturate(color.a + grid * edgeFade * depth * 0.3 * intensity);

    return color * inColor;
}

// Grid only, no scene sampling (for backdrop layers)
float4 PS_RainbowGridOnly(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : SV_TARGET0
{
    float4 color = float4(0, 0, 0, 0);

    float horizonY = 0.55;
    float belowHorizon = saturate((uv.y - horizonY) / (1.0 - horizonY));
    float depth = belowHorizon * belowHorizon;
    float scroll = time * speed * 0.5;

    float lineY = frac(log(belowHorizon + 0.01) * 3.0 + scroll);
    float hLine = smoothstep(0.08, 0.0, abs(lineY - 0.5)) * belowHorizon;

    float centerX = (uv.x - 0.5) / (belowHorizon + 0.05);
    float lineX = frac(centerX + scroll * 0.3);
    float vLine = smoothstep(0.06, 0.0, abs(lineX - 0.5)) * belowHorizon;

    float grid = saturate(hLine + vLine);

    float hue = (time * 0.2 + uv.x * 0.3 + belowHorizon * 0.5) % 1.0;
    float3 rainbow;
    rainbow.r = abs(hue * 6.0 - 3.0) - 1.0;
    rainbow.g = 2.0 - abs(hue * 6.0 - 2.0);
    rainbow.b = 2.0 - abs(hue * 6.0 - 4.0);
    rainbow = saturate(rainbow);

    float edgeFade = saturate(1.0 - abs(uv.x - 0.5) * 1.5);

    color.rgb = rainbow * grid * edgeFade * depth * intensity;
    color.a = grid * edgeFade * depth * intensity;

    return color * inColor;
}

//-----------------------------------------------------------------------------
// Techniques.
//-----------------------------------------------------------------------------

technique RainbowBackdropFlatGrid
{
    pass
    {
        PixelShader = compile PS_2_SHADER_COMPILER PS_RainbowBackdropFlatGrid();
    }
}

technique RainbowGridOnly
{
    pass
    {
        PixelShader = compile PS_2_SHADER_COMPILER PS_RainbowGridOnly();
    }
}
