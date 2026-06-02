module KirbyBoss

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/KirbyBoss" KirbyBoss(x::Integer, y::Integer, attackCooldown::Number=2.0, bossMusic::String="event:/pusheen/music/miniboss/main0", health::Integer=15)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(KirbyBoss)
)

function Ahorn.selection(entity::KirbyBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbyBoss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
