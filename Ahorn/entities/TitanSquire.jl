module TitanSquire

using ..Ahorn, Maple

@mapdef Entity "DZ/TitanSquire" TitanSquire(x::Integer, y::Integer, attackRange::Integer=80, detectionRange::Integer=180, health::Integer=4, moveSpeed::Integer=70, patrolDistance::Integer=120)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(TitanSquire)
)

function Ahorn.selection(entity::TitanSquire)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TitanSquire, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
