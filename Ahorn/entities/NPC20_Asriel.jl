module NPC20_Asriel

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/NPC20_Asriel" NPC20_Asriel(x::Integer, y::Integer, dialogKey::String="ingeste_asriel_20_final", flagName::String="asriel_20_final", spriteId::String="asriel", talking::Bool=false)

const placements = Ahorn.PlacementDict(
    "NPC20_Asriel" => Ahorn.EntityPlacement(NPC20_Asriel)
)

function Ahorn.selection(entity::NPC20_Asriel)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC20_Asriel, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/asriel/idle00", entity.x, entity.y)
end

end
