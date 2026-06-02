module NPC05_Oshiro_Clutter

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/NPC05_Oshiro_Clutter" NPC05_Oshiro_Clutter(x::Integer, y::Integer, dialogKey::String="ingeste_oshiro_05_clutter", flagName::String="oshiro_05_clutter", spriteId::String="oshiro")

const placements = Ahorn.PlacementDict(
    "NPC05_Oshiro_Clutter" => Ahorn.EntityPlacement(NPC05_Oshiro_Clutter)
)

function Ahorn.selection(entity::NPC05_Oshiro_Clutter)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC05_Oshiro_Clutter, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/oshiro/oshiro25", entity.x, entity.y)
end

end
