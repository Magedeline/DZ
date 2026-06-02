module NPC17_Toriel

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/NPC17_Toriel" NPC17_Toriel(x::Integer, y::Integer, dialogKey::String="ingeste_toriel_17_final", flagName::String="toriel_17_final", spriteId::String="toriel", talking::Bool=false)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(NPC17_Toriel)
)

function Ahorn.selection(entity::NPC17_Toriel)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC17_Toriel, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/toriel/idle00", entity.x, entity.y)
end

end
