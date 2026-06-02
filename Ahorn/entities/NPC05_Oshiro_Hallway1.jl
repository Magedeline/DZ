module NPC05_Oshiro_Hallway1

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/NPC05_Oshiro_Hallway1" NPC05_Oshiro_Hallway1(x::Integer, y::Integer, dialogKey::String="ingeste_oshiro_05_hallway1", flagName::String="oshiro_05_hallway1", spriteId::String="oshiro")

const placements = Ahorn.PlacementDict(
    "NPC05_Oshiro_Hallway1" => Ahorn.EntityPlacement(NPC05_Oshiro_Hallway1)
)

function Ahorn.selection(entity::NPC05_Oshiro_Hallway1)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC05_Oshiro_Hallway1, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/oshiro/oshiro25", entity.x, entity.y)
end

end
