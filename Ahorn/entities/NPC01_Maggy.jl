module NPC01_Maggy

using ..Ahorn, Maple

@mapdef Entity "DZ/NPC01_Maggy" NPC01_Maggy(x::Integer, y::Integer, currentConversation::Integer=0, dialogKey::String="ingeste_maggy_01_conversation", flagName::String="maggy_01_talked", spriteId::String="magolor")

const placements = Ahorn.PlacementDict(
    "NPC01_Maggy" => Ahorn.EntityPlacement(NPC01_Maggy)
)

function Ahorn.selection(entity::NPC01_Maggy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC01_Maggy, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/magolor/idle00", entity.x, entity.y)
end

end
