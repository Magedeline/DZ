module ClutterDoor

using ..Ahorn, Maple

@mapdef Entity "DZ/ClutterDoor" ClutterDoor(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "ClutterDoor" => Ahorn.EntityPlacement(ClutterDoor)
)

function Ahorn.selection(entity::ClutterDoor)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ClutterDoor, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
