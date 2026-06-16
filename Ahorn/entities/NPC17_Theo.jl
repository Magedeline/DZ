module NPC17_Theo

using ..Ahorn, Maple

@mapdef Entity "DZ/NPC17_Theo" NPC17_Theo(x::Integer, y::Integer, dialogKey::String="ingeste_theo_17_final", flagName::String="theo_17_final", spriteId::String="theo", talking::Bool=false)

const placements = Ahorn.PlacementDict(
    "NPC17_Theo" => Ahorn.EntityPlacement(NPC17_Theo)
)

function Ahorn.selection(entity::NPC17_Theo)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC17_Theo, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/theo/theo00", entity.x, entity.y)
end

end
