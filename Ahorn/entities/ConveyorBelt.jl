module ConveyorBelt

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/ConveyorBelt" ConveyorBelt(x::Integer, y::Integer, speed::Number=60.0, width::Integer=48)

const placements = Ahorn.PlacementDict(
    "right" => Ahorn.EntityPlacement(ConveyorBelt),
    "left" => Ahorn.EntityPlacement(ConveyorBelt),
    "fast_right" => Ahorn.EntityPlacement(ConveyorBelt)
)

function Ahorn.selection(entity::ConveyorBelt)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ConveyorBelt, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.4), (0.7, 0.7, 0.7, 0.8))
end

end
