module MovingLavaBlock

using ..Ahorn, Maple

@mapdef Entity "DZ/MovingLavaBlock" MovingLavaBlock(x::Integer, y::Integer, height::Integer=8, nodeIndex::Integer=0, width::Integer=8)

const placements = Ahorn.PlacementDict(
    "chara_moving_lava_block" => Ahorn.EntityPlacement(MovingLavaBlock)
)

function Ahorn.selection(entity::MovingLavaBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MovingLavaBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=1, max=1
# Basic node rendering not implemented in auto-generated plugin

end
