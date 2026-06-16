module HopesAndDreamsBlock

using ..Ahorn, Maple

@mapdef Entity "DZ/HopesAndDreamsBlock" HopesAndDreamsBlock(x::Integer, y::Integer, below::Bool=false, fastMoving::Bool=false, height::Integer=16, oneUse::Bool=false, primaryColor::String="FFD700", secondaryColor::String="FF69B4", showStars::Bool=true, tertiaryColor::String="FF4500", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "HopesAndDreamsBlock" => Ahorn.EntityPlacement(HopesAndDreamsBlock)
)

function Ahorn.selection(entity::HopesAndDreamsBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HopesAndDreamsBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=0, max=1
# Basic node rendering not implemented in auto-generated plugin

end
