module BossTier1

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/BossTier1" BossTier1(x::Integer, y::Integer, arenaRadius::Number=150.0, bossType::String="BasicEnemy", gimmick::Integer=0, health::Integer=100, speed::Number=20.0, tier::Integer=1)

const placements = Ahorn.PlacementDict(
    "boss_tier_1" => Ahorn.EntityPlacement(BossTier1),
    "boss_tier_2" => Ahorn.EntityPlacement(BossTier1),
    "boss_tier_3" => Ahorn.EntityPlacement(BossTier1),
    "boss_tier_4" => Ahorn.EntityPlacement(BossTier1),
    "boss_tier_5" => Ahorn.EntityPlacement(BossTier1),
    "boss_tier_6" => Ahorn.EntityPlacement(BossTier1)
)

function Ahorn.selection(entity::BossTier1)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BossTier1, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/boss/tier1", entity.x, entity.y)
end

end
