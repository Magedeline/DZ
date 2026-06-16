module KirbyMidBoss

using ..Ahorn, Maple

@mapdef Entity "DZ/KirbyMidBoss" KirbyMidBoss(x::Integer, y::Integer, arenaHeight::Integer=180, arenaWidth::Integer=320, bossType::Integer=0, canBeInhaled::Bool=false, maxHealth::Integer=100, powerType::Integer=0)

const placements = Ahorn.PlacementDict(
    "Whispy Woods" => Ahorn.EntityPlacement(KirbyMidBoss),
    "Kracko" => Ahorn.EntityPlacement(KirbyMidBoss),
    "Mr. Frosty" => Ahorn.EntityPlacement(KirbyMidBoss),
    "Bonkers" => Ahorn.EntityPlacement(KirbyMidBoss),
    "Bugzzy" => Ahorn.EntityPlacement(KirbyMidBoss),
    "Fire Lion" => Ahorn.EntityPlacement(KirbyMidBoss),
    "Iron Mam" => Ahorn.EntityPlacement(KirbyMidBoss),
    "Grand Wheely" => Ahorn.EntityPlacement(KirbyMidBoss),
    "Box Boxer" => Ahorn.EntityPlacement(KirbyMidBoss),
    "Master Hand" => Ahorn.EntityPlacement(KirbyMidBoss)
)

function Ahorn.selection(entity::KirbyMidBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbyMidBoss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
