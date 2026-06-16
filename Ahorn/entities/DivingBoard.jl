module DivingBoard

using ..Ahorn, Maple

@mapdef Entity "DZ/DivingBoard" DivingBoard(x::Integer, y::Integer, launchSpeed::Number=-300.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(DivingBoard),
    "high_launch" => Ahorn.EntityPlacement(DivingBoard)
)

function Ahorn.selection(entity::DivingBoard)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DivingBoard, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
