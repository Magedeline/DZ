module RainbowBlackholeBG

using ..Ahorn, Maple

@mapdef Effect "MaggyHelper/RainbowBlackholeBG" RainbowBlackholeBG(x::Integer, y::Integer, alpha::Number=1.0, animationLoops::Bool=true, animationMode::String="None", animationScale::Number=1.0, chromaticAberration::Number=0.0, color::String="FFFFFF", distortionAmount::Number=0.0, distortionFrequency::Number=2.0, exclude::String="", fadeIn::Bool=false, fadeOut::Bool=false, fadeX::String="", fadeY::String="", flag::String="", flipX::Bool=false, flipY::Bool=false, frameDelay::Number=0.08, glitchAmount::Number=0.0, instantIn::Bool=false, instantOut::Bool=false, loopX::Bool=true, loopY::Bool=true, notFlag::String="", only::String="*", scrollX::Number=1.0, scrollY::Number=1.0, speedX::Number=0.0, speedY::Number=0.0, strength::String="Mild", tag::String="")

const placements = Ahorn.PlacementDict(
    "rainbow_blackhole_bg" => Ahorn.EntityPlacement(RainbowBlackholeBG),
    "rainbow_blackhole_soul" => Ahorn.EntityPlacement(RainbowBlackholeBG),
    "rainbow_blackhole_zero" => Ahorn.EntityPlacement(RainbowBlackholeBG),
    "rainbow_blackhole_void" => Ahorn.EntityPlacement(RainbowBlackholeBG),
    "rainbow_blackhole_zero_glitch_distortion" => Ahorn.EntityPlacement(RainbowBlackholeBG),
    "rainbow_blackhole_darkmatter_glitch_distortion" => Ahorn.EntityPlacement(RainbowBlackholeBG)
)

function Ahorn.selection(entity::RainbowBlackholeBG)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RainbowBlackholeBG, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
