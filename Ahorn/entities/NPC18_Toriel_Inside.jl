module NPC18_Toriel_Inside

using ..Ahorn, Maple

@mapdef Entity "DZ/NPC18_Toriel_Inside" NPC18_Toriel_Inside(x::Integer, y::Integer, dialogKey::String="ingeste_toriel_18_inside", flagName::String="toriel_18_inside", spriteId::String="toriel")

const placements = Ahorn.PlacementDict(
    "NPC18_Toriel_Inside" => Ahorn.EntityPlacement(NPC18_Toriel_Inside)
)

function Ahorn.selection(entity::NPC18_Toriel_Inside)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC18_Toriel_Inside, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/toriel/idle00", entity.x, entity.y)
end

end
