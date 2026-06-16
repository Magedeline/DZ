module RevolverTarget

using ..Ahorn, Maple

@mapdef Entity "DZ/RevolverTarget" RevolverTarget(x::Integer, y::Integer, moveSpeed::Integer=50, points::Integer=100, resetTime::Number=3.0, showTime::Number=2.0, targetType::String="Static")

const placements = Ahorn.PlacementDict(
    "static" => Ahorn.EntityPlacement(RevolverTarget),
    "popup" => Ahorn.EntityPlacement(RevolverTarget),
    "moving" => Ahorn.EntityPlacement(RevolverTarget)
)

function Ahorn.selection(entity::RevolverTarget)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RevolverTarget, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
