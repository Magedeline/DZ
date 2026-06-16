module SwingingDoor

using ..Ahorn, Maple

@mapdef Entity "DZ/SwingingDoor" SwingingDoor(x::Integer, y::Integer, autoCloseTime::Number=2.0, isDoubleDoor::Bool=true, isLocked::Bool=false, knockbackForce::Integer=150, swingSpeed::Number=3.0)

const placements = Ahorn.PlacementDict(
    "unlocked" => Ahorn.EntityPlacement(SwingingDoor),
    "locked" => Ahorn.EntityPlacement(SwingingDoor)
)

function Ahorn.selection(entity::SwingingDoor)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SwingingDoor, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
