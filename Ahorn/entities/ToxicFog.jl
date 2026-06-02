module ToxicFog

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/ToxicFog" ToxicFog(x::Integer, y::Integer, damageInterval::Number=1.0, height::Integer=64, width::Integer=64)

const placements = Ahorn.PlacementDict(
    "ToxicFog" => Ahorn.EntityPlacement(ToxicFog),
    "ToxicFog_dense" => Ahorn.EntityPlacement(ToxicFog)
)

function Ahorn.selection(entity::ToxicFog)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ToxicFog, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
