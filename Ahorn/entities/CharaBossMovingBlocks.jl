module CharaBossMovingBlocks

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/CharaBossMovingBlocks" CharaBossMovingBlocks(x::Integer, y::Integer, height::Integer=8, nodeIndex::Integer=0, width::Integer=8)

const placements = Ahorn.PlacementDict(
    "chara_moving_block" => Ahorn.EntityPlacement(CharaBossMovingBlocks)
)

function Ahorn.selection(entity::CharaBossMovingBlocks)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CharaBossMovingBlocks, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=1, max=1
# Basic node rendering not implemented in auto-generated plugin

end
