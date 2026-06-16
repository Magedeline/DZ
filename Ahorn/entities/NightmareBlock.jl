module NightmareBlock

using ..Ahorn, Maple

@mapdef Entity "DZ/NightmareBlock" NightmareBlock(x::Integer, y::Integer, below::Bool=false, fastMoving::Bool=false, height::Integer=8, oneUse::Bool=false, width::Integer=8)

const placements = Ahorn.PlacementDict(
    "nightmare_block" => Ahorn.EntityPlacement(NightmareBlock)
)

function Ahorn.selection(entity::NightmareBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NightmareBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=0, max=1
# Basic node rendering not implemented in auto-generated plugin

end
