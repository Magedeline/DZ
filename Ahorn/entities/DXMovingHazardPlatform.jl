module DXMovingHazardPlatform

using ..Ahorn, Maple

@mapdef Entity "DZ/DXMovingHazardPlatform" DXMovingHazardPlatform(x::Integer, y::Integer, hazardSides::Bool=true, pauseTime::Number=0.5, speed::Number=80.0, width::Integer=48)

const placements = Ahorn.PlacementDict(
    "DXMovingHazardPlatform" => Ahorn.EntityPlacement(DXMovingHazardPlatform),
    "DXMovingHazardPlatform_Fast" => Ahorn.EntityPlacement(DXMovingHazardPlatform)
)

function Ahorn.selection(entity::DXMovingHazardPlatform)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DXMovingHazardPlatform, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=1, max=8
# Basic node rendering not implemented in auto-generated plugin

end
