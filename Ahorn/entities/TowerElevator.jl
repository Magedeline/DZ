module TowerElevator

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/TowerElevator" TowerElevator(x::Integer, y::Integer, elevatorId::String="", height::Integer=8, moveSpeed::Integer=80, waitTime::Number=1.0, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(TowerElevator)
)

function Ahorn.selection(entity::TowerElevator)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TowerElevator, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
