module NPC09_SavePoint

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/NPC09_SavePoint" NPC09_SavePoint(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "NPC09_SavePoint" => Ahorn.EntityPlacement(NPC09_SavePoint),
    "NPC09_SavePoint_Trap" => Ahorn.EntityPlacement(NPC09_SavePoint)
)

function Ahorn.selection(entity::NPC09_SavePoint)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC09_SavePoint, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/savepoint/save00", entity.x, entity.y)
end

end
