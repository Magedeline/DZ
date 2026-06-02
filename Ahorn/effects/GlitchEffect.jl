module GlitchEffect

using ..Ahorn, Maple

@mapdef Effect "MaggyHelper/GlitchEffect" GlitchEffect(x::Integer, y::Integer, amplitude::Number=0.05, glitchAmount::Number=0.0)

const placements = Ahorn.PlacementDict(
    "glitch" => Ahorn.EntityPlacement(GlitchEffect)
)

function Ahorn.selection(entity::GlitchEffect)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GlitchEffect, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
