module DXDimensionalWarpBlock

using ..Ahorn, Maple

@mapdef Entity "DZ/DXDimensionalWarpBlock" DXDimensionalWarpBlock(x::Integer, y::Integer, cooldown::Number=1.0, height::Integer=16, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "DXDimensionalWarpBlock" => Ahorn.EntityPlacement(DXDimensionalWarpBlock)
)

function Ahorn.selection(entity::DXDimensionalWarpBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DXDimensionalWarpBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.0, 0.5, 1.0, 0.4), (0.0, 0.7, 1.0, 0.8))
end

# Nodes: min=1, max=1
# Basic node rendering not implemented in auto-generated plugin

end
