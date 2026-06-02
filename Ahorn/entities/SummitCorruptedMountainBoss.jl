module SummitCorruptedMountainBoss

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/SummitCorruptedMountainBoss" SummitCorruptedMountainBoss(x::Integer, y::Integer, health::Integer=1500, maxHealth::Integer=1500)

const placements = Ahorn.PlacementDict(
    "summit_corrupted_mountain_boss" => Ahorn.EntityPlacement(SummitCorruptedMountainBoss)
)

function Ahorn.selection(entity::SummitCorruptedMountainBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SummitCorruptedMountainBoss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
