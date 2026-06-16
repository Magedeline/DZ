module NPC05_Oshiro_Rooftop

using ..Ahorn, Maple

@mapdef Entity "DZ/NPC05_Oshiro_Rooftop" NPC05_Oshiro_Rooftop(x::Integer, y::Integer, dialogKey::String="ingeste_oshiro_05_rooftop", flagName::String="oshiro_05_rooftop", spriteId::String="oshiro")

const placements = Ahorn.PlacementDict(
    "NPC05_Oshiro_Rooftop" => Ahorn.EntityPlacement(NPC05_Oshiro_Rooftop)
)

function Ahorn.selection(entity::NPC05_Oshiro_Rooftop)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC05_Oshiro_Rooftop, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/oshiro/oshiro25", entity.x, entity.y)
end

end
