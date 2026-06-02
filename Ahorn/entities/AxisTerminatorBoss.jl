module AxisTerminatorBoss

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/AxisTerminatorBoss" AxisTerminatorBoss(x::Integer, y::Integer, armorPieces::Integer=3, health::Integer=500, maxHealth::Integer=500)

const placements = Ahorn.PlacementDict(
    "axis_terminator_boss" => Ahorn.EntityPlacement(AxisTerminatorBoss)
)

function Ahorn.selection(entity::AxisTerminatorBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AxisTerminatorBoss, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/axis/terminator_idle00", entity.x, entity.y)
end

end
