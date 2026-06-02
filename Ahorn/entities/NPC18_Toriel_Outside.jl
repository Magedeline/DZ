module NPC18_Toriel_Outside

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/NPC18_Toriel_Outside" NPC18_Toriel_Outside(x::Integer, y::Integer, dialogKey::String="ingeste_toriel_18_outside", flagName::String="toriel_18_outside", spriteId::String="toriel")

const placements = Ahorn.PlacementDict(
    "NPC18_Toriel_Outside" => Ahorn.EntityPlacement(NPC18_Toriel_Outside)
)

function Ahorn.selection(entity::NPC18_Toriel_Outside)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC18_Toriel_Outside, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/toriel/idle00", entity.x, entity.y)
end

end
