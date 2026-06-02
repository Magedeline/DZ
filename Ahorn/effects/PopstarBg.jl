module PopstarBg

using ..Ahorn, Maple

@mapdef Effect "MaggyHelper/PopstarBg" PopstarBg(x::Integer, y::Integer, alpha::Number=1.0, color::String="FFFFFF", exclude::String="", fadeIn::Bool=false, fadeOut::Bool=false, fadeX::String="", fadeY::String="", flag::String="", flipX::Bool=false, flipY::Bool=false, frameDelay::Number=0.08, glowIntensity::Number=0.0, instantIn::Bool=false, instantOut::Bool=false, loopX::Bool=true, loopY::Bool=true, loops::Bool=true, notFlag::String="", only::String="*", pingPong::Bool=false, pulseAmount::Number=0.0, pulseSpeed::Number=1.0, rainbowSpeed::Number=1.0, rotationSpeed::Number=0.0, scale::Number=1.0, scrollSpeedX::Number=0.0, scrollSpeedY::Number=0.0, scrollX::Number=1.0, scrollY::Number=1.0, speedX::Number=0.0, speedY::Number=0.0, style::String="Normal", tag::String="", tintColor::String="FFFFFF")

const placements = Ahorn.PlacementDict(
    "popstar_normal" => Ahorn.EntityPlacement(PopstarBg),
    "popstar_dreamy" => Ahorn.EntityPlacement(PopstarBg),
    "popstar_sunset" => Ahorn.EntityPlacement(PopstarBg),
    "popstar_night" => Ahorn.EntityPlacement(PopstarBg),
    "popstar_rainbow" => Ahorn.EntityPlacement(PopstarBg)
)

function Ahorn.selection(entity::PopstarBg)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PopstarBg, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
