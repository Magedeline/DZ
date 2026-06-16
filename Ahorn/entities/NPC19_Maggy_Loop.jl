module NPC19_Maggy_Loop

using ..Ahorn, Maple

@mapdef Entity "DZ/NPC19_Maggy_Loop" NPC19_Maggy_Loop(x::Integer, y::Integer, dialogKey::String="ingeste_maggy_19_loop", flagName::String="maggy_19_loop", spriteId::String="magolor")

const placements = Ahorn.PlacementDict(
    "NPC19_Maggy_Loop" => Ahorn.EntityPlacement(NPC19_Maggy_Loop)
)

function Ahorn.selection(entity::NPC19_Maggy_Loop)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC19_Maggy_Loop, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/magolor/idle00", entity.x, entity.y)
end

end
