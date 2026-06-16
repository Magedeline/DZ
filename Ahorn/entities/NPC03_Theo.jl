module NPC03_Theo

using ..Ahorn, Maple

@mapdef Entity "DZ/NPC03_Theo" NPC03_Theo(x::Integer, y::Integer, dialogKey::String="ingeste_theo_03_conversation", flagName::String="theo_03_talked", spriteId::String="theo")

const placements = Ahorn.PlacementDict(
    "NPC03_Theo" => Ahorn.EntityPlacement(NPC03_Theo)
)

function Ahorn.selection(entity::NPC03_Theo)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC03_Theo, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/theo/theo00", entity.x, entity.y)
end

end
