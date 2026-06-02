module AnotherVesselCorruptedBoss

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/AnotherVesselCorruptedBoss" AnotherVesselCorruptedBoss(x::Integer, y::Integer, health::Integer=1000, maxHealth::Integer=1000)

const placements = Ahorn.PlacementDict(
    "another_vessel_corrupted_boss" => Ahorn.EntityPlacement(AnotherVesselCorruptedBoss)
)

function Ahorn.selection(entity::AnotherVesselCorruptedBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AnotherVesselCorruptedBoss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
