module DigitalPortal

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/DigitalPortal" DigitalPortal(x::Integer, y::Integer, destinationId::String="", isTwoWay::Bool=true, transportDelay::Number=0.5)

const placements = Ahorn.PlacementDict(
    "two_way" => Ahorn.EntityPlacement(DigitalPortal),
    "one_way" => Ahorn.EntityPlacement(DigitalPortal)
)

function Ahorn.selection(entity::DigitalPortal)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DigitalPortal, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
