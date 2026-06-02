module RealityGlitch

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/RealityGlitch" RealityGlitch(x::Integer, y::Integer, glitchIntensity::Number=0.5, height::Integer=32, teleportChance::Number=0.1, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "minor" => Ahorn.EntityPlacement(RealityGlitch)
)

function Ahorn.selection(entity::RealityGlitch)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RealityGlitch, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
