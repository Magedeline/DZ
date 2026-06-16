module MetaKnightBoss

using ..Ahorn, Maple

@mapdef Entity "DZ/MetaKnightBoss" MetaKnightBoss(x::Integer, y::Integer, attackCooldown::Number=0.8, bossMusic::String="event:/pusheen/music/lvl13/metarminator_kight", health::Integer=20)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(MetaKnightBoss)
)

function Ahorn.selection(entity::MetaKnightBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MetaKnightBoss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
