module RealityAnchor

using ..Ahorn, Maple

@mapdef Entity "DZ/RealityAnchor" RealityAnchor(x::Integer, y::Integer, stabilityRadius::Integer=100)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(RealityAnchor)
)

function Ahorn.selection(entity::RealityAnchor)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RealityAnchor, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
