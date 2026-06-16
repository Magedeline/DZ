module NPC_Event

using ..Ahorn, Maple

@mapdef Entity "DZ/NPC_Event" NPC_Event(x::Integer, y::Integer, dialogKey::String="", eventId::String="", flagName::String="", spriteId::String="theo")

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(NPC_Event),
    "theo" => Ahorn.EntityPlacement(NPC_Event),
    "maggy" => Ahorn.EntityPlacement(NPC_Event),
    "chara" => Ahorn.EntityPlacement(NPC_Event)
)

function Ahorn.selection(entity::NPC_Event)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC_Event, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/theo/idle00", entity.x, entity.y)
end

end
