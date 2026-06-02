module GlitchGlider

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/GlitchGlider" GlitchGlider(x::Integer, y::Integer, glitchColor1::String="FF00FF", glitchColor2::String="00FFFF", maxUses::Integer=5, teleportRange::Number=300.0, throwSpeed::Number=200.0)

const placements = Ahorn.PlacementDict(
    "MaggyHelper/GlitchGlider" => Ahorn.EntityPlacement(GlitchGlider)
)

function Ahorn.selection(entity::GlitchGlider)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GlitchGlider, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/Ingeste/glitchGlider/available00", entity.x, entity.y)
end

end
