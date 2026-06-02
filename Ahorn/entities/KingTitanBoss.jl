module KingTitanBoss

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/KingTitanBoss" KingTitanBoss(x::Integer, y::Integer, currentPhase::Integer=1, health::Integer=2000, maxHealth::Integer=2000)

const placements = Ahorn.PlacementDict(
    "king_titan_boss" => Ahorn.EntityPlacement(KingTitanBoss)
)

function Ahorn.selection(entity::KingTitanBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KingTitanBoss, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/kingtitan/titan_idle00", entity.x, entity.y)
end

end
