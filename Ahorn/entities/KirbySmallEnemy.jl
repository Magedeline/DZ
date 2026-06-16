module KirbySmallEnemy

using ..Ahorn, Maple

@mapdef Entity "DZ/KirbySmallEnemy" KirbySmallEnemy(x::Integer, y::Integer, attackRange::Integer=30, canBeInhaled::Bool=true, detectionRange::Integer=100, facingRight::Bool=true, maxHealth::Integer=1, moveSpeed::Integer=30, patrolDistance::Integer=64, powerType::Integer=0, variant::Integer=0)

const placements = Ahorn.PlacementDict(
    "Waddle Dee" => Ahorn.EntityPlacement(KirbySmallEnemy),
    "Waddle Doo (Beam)" => Ahorn.EntityPlacement(KirbySmallEnemy),
    "Hot Head (Fire)" => Ahorn.EntityPlacement(KirbySmallEnemy),
    "Chilly (Ice)" => Ahorn.EntityPlacement(KirbySmallEnemy),
    "Sparky (Spark)" => Ahorn.EntityPlacement(KirbySmallEnemy),
    "Rocky (Stone)" => Ahorn.EntityPlacement(KirbySmallEnemy),
    "Sir Kibble (Cutter)" => Ahorn.EntityPlacement(KirbySmallEnemy),
    "Poppy (Bomb)" => Ahorn.EntityPlacement(KirbySmallEnemy),
    "Wheelie (Wheel)" => Ahorn.EntityPlacement(KirbySmallEnemy),
    "Gordo (Invincible)" => Ahorn.EntityPlacement(KirbySmallEnemy)
)

function Ahorn.selection(entity::KirbySmallEnemy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbySmallEnemy, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
