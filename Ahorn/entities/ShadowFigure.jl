module ShadowFigure

using ..Ahorn, Maple

@mapdef Entity "DZ/ShadowFigure" ShadowFigure(x::Integer, y::Integer, detectionRange::Integer=150, followDistance::Integer=80, isHostile::Bool=false)

const placements = Ahorn.PlacementDict(
    "passive" => Ahorn.EntityPlacement(ShadowFigure),
    "hostile" => Ahorn.EntityPlacement(ShadowFigure)
)

function Ahorn.selection(entity::ShadowFigure)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ShadowFigure, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
