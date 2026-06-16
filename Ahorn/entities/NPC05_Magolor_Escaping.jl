module NPC05_Magolor_Escaping

using ..Ahorn, Maple

@mapdef Entity "DZ/NPC05_Magolor_Escaping" NPC05_Magolor_Escaping(x::Integer, y::Integer, dialogKey::String="CH5_MAGGY_ESCAPING", flagName::String="resort_maggy_escaped", spriteId::String="magolor")

const placements = Ahorn.PlacementDict(
    "NPC05_Magolor_Escaping" => Ahorn.EntityPlacement(NPC05_Magolor_Escaping)
)

function Ahorn.selection(entity::NPC05_Magolor_Escaping)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC05_Magolor_Escaping, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/magolor/idle00", entity.x, entity.y)
end

end
