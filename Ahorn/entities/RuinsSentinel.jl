module RuinsSentinel

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/RuinsSentinel" RuinsSentinel(x::Integer, y::Integer, attackRange::Integer=60, detectionRange::Integer=150, health::Integer=3, moveSpeed::Integer=50, patrolDistance::Integer=100)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(RuinsSentinel)
)

function Ahorn.selection(entity::RuinsSentinel)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RuinsSentinel, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
