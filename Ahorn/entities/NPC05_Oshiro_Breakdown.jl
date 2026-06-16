module NPC05_Oshiro_Breakdown

using ..Ahorn, Maple

@mapdef Entity "DZ/NPC05_Oshiro_Breakdown" NPC05_Oshiro_Breakdown(x::Integer, y::Integer, dialogKey::String="ingeste_oshiro_05_breakdown", flagName::String="oshiro_05_breakdown", spriteId::String="oshiro")

const placements = Ahorn.PlacementDict(
    "NPC05_Oshiro_Breakdown" => Ahorn.EntityPlacement(NPC05_Oshiro_Breakdown)
)

function Ahorn.selection(entity::NPC05_Oshiro_Breakdown)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC05_Oshiro_Breakdown, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/oshiro/oshiro25", entity.x, entity.y)
end

end
