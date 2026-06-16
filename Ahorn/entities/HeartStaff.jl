module HeartStaff

using ..Ahorn, Maple

@mapdef Entity "DZ/HeartStaff" HeartStaff(x::Integer, y::Integer, staffColor::String="red", staffId::String="")

const placements = Ahorn.PlacementDict(
    "red" => Ahorn.EntityPlacement(HeartStaff),
    "blue" => Ahorn.EntityPlacement(HeartStaff),
    "yellow" => Ahorn.EntityPlacement(HeartStaff),
    "green" => Ahorn.EntityPlacement(HeartStaff),
    "purple" => Ahorn.EntityPlacement(HeartStaff),
    "orange" => Ahorn.EntityPlacement(HeartStaff),
    "pink" => Ahorn.EntityPlacement(HeartStaff)
)

function Ahorn.selection(entity::HeartStaff)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HeartStaff, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
