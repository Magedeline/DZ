module DimensionalRift

using ..Ahorn, Maple

@mapdef Entity "DZ/DimensionalRift" DimensionalRift(x::Integer, y::Integer, bidirectional::Bool=true, riftId::String="", targetRiftId::String="", targetRoom::String="", transitionColor::String="800080")

const placements = Ahorn.PlacementDict(
    "Dimensional Rift" => Ahorn.EntityPlacement(DimensionalRift)
)

function Ahorn.selection(entity::DimensionalRift)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DimensionalRift, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=1, max=1
# Basic node rendering not implemented in auto-generated plugin

end
