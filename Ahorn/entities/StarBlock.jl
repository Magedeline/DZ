module StarBlock

using ..Ahorn, Maple

@mapdef Entity "DZ/StarBlock" StarBlock(x::Integer, y::Integer, height::Integer=8, width::Integer=8)

const placements = Ahorn.PlacementDict(
    "StarBlock" => Ahorn.EntityPlacement(StarBlock)
)

function Ahorn.selection(entity::StarBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::StarBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
