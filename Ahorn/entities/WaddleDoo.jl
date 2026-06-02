module WaddleDoo

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/WaddleDoo" WaddleDoo(x::Integer, y::Integer, attackCooldown::Number=3.0, canBeInhaled::Bool=true, health::Integer=2, moveSpeed::Number=25.0)

const placements = Ahorn.PlacementDict(
    "WaddleDoo" => Ahorn.EntityPlacement(WaddleDoo)
)

function Ahorn.selection(entity::WaddleDoo)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WaddleDoo, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
