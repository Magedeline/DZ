module WaddleDee

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/WaddleDee" WaddleDee(x::Integer, y::Integer, canBeInhaled::Bool=true, health::Integer=1, moveSpeed::Number=30.0, patrolDistance::Number=50.0)

const placements = Ahorn.PlacementDict(
    "WaddleDee" => Ahorn.EntityPlacement(WaddleDee)
)

function Ahorn.selection(entity::WaddleDee)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WaddleDee, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
