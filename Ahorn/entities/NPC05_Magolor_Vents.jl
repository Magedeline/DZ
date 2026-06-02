module NPC05_Magolor_Vents

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/NPC05_Magolor_Vents" NPC05_Magolor_Vents(x::Integer, y::Integer, dialogKey::String="CH5_MAGGY_VENTS", flagName::String="magolorVentsTalked", spriteId::String="magolor")

const placements = Ahorn.PlacementDict(
    "NPC05_Magolor_Vents" => Ahorn.EntityPlacement(NPC05_Magolor_Vents)
)

function Ahorn.selection(entity::NPC05_Magolor_Vents)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC05_Magolor_Vents, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/magolor/idle00", entity.x, entity.y)
end

end
