module NPC03_Maggy

using ..Ahorn, Maple

@mapdef Entity "DZ/NPC03_Maggy" NPC03_Maggy(x::Integer, y::Integer, dialogKey::String="ingeste_maggy_03_conversation", flagName::String="maggy_03_talked", spriteId::String="magolor")

const placements = Ahorn.PlacementDict(
    "NPC03_Maggy" => Ahorn.EntityPlacement(NPC03_Maggy)
)

function Ahorn.selection(entity::NPC03_Maggy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC03_Maggy, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/magolor/idle00", entity.x, entity.y)
end

end
