module TennaTVBoss

using ..Ahorn, Maple

@mapdef Entity "DZ/TennaTVBoss" TennaTVBoss(x::Integer, y::Integer, health::Integer=300, maxHealth::Integer=300)

const placements = Ahorn.PlacementDict(
    "tenna_tv_boss" => Ahorn.EntityPlacement(TennaTVBoss)
)

function Ahorn.selection(entity::TennaTVBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TennaTVBoss, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/tenna/tv_idle00", entity.x, entity.y)
end

end
