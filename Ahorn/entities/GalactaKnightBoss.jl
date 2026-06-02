module GalactaKnightBoss

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/GalactaKnightBoss" GalactaKnightBoss(x::Integer, y::Integer, health::Integer=1700, maxHealth::Integer=1700)

const placements = Ahorn.PlacementDict(
    "galacta_knight_boss" => Ahorn.EntityPlacement(GalactaKnightBoss)
)

function Ahorn.selection(entity::GalactaKnightBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GalactaKnightBoss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
