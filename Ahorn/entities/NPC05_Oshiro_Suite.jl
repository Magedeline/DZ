module NPC05_Oshiro_Suite

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/NPC05_Oshiro_Suite" NPC05_Oshiro_Suite(x::Integer, y::Integer, dialogKey::String="ingeste_oshiro_05_suite", flagName::String="oshiro_05_suite", spriteId::String="oshiro")

const placements = Ahorn.PlacementDict(
    "NPC05_Oshiro_Suite" => Ahorn.EntityPlacement(NPC05_Oshiro_Suite)
)

function Ahorn.selection(entity::NPC05_Oshiro_Suite)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC05_Oshiro_Suite, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/oshiro/oshiro25", entity.x, entity.y)
end

end
