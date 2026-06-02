module HeartCompanion

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/HeartCompanion" HeartCompanion(x::Integer, y::Integer, slotIndex::Integer=0)

const placements = Ahorn.PlacementDict(
    "leader" => Ahorn.EntityPlacement(HeartCompanion),
    "guardian" => Ahorn.EntityPlacement(HeartCompanion),
    "striker" => Ahorn.EntityPlacement(HeartCompanion),
    "sniper" => Ahorn.EntityPlacement(HeartCompanion),
    "medic" => Ahorn.EntityPlacement(HeartCompanion),
    "surge" => Ahorn.EntityPlacement(HeartCompanion),
    "purifier" => Ahorn.EntityPlacement(HeartCompanion)
)

function Ahorn.selection(entity::HeartCompanion)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HeartCompanion, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
