module Gordo

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/Gordo" Gordo(x::Integer, y::Integer, moveDistance::Number=48.0, moveSpeed::Number=40.0, movementType::String="Stationary", pauseDuration::Number=0.5)

const placements = Ahorn.PlacementDict(
    "stationary" => Ahorn.EntityPlacement(Gordo),
    "horizontal" => Ahorn.EntityPlacement(Gordo),
    "vertical" => Ahorn.EntityPlacement(Gordo),
    "diagonal" => Ahorn.EntityPlacement(Gordo),
    "circular" => Ahorn.EntityPlacement(Gordo)
)

function Ahorn.selection(entity::Gordo)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Gordo, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
