module StickyWall

using ..Ahorn, Maple

@mapdef Entity "DZ/StickyWall" StickyWall(x::Integer, y::Integer, height::Integer=32, stickDuration::Number=5.0, width::Integer=8)

const placements = Ahorn.PlacementDict(
    "StickyWall" => Ahorn.EntityPlacement(StickyWall),
    "StickyWall_infinite" => Ahorn.EntityPlacement(StickyWall)
)

function Ahorn.selection(entity::StickyWall)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::StickyWall, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.2, 0.6, 0.2, 0.3), (0.3, 0.8, 0.3, 0.6))
end

end
