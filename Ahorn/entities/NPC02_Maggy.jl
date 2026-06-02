module NPC02_Maggy

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/NPC02_Maggy" NPC02_Maggy(x::Integer, y::Integer, dialogKey::String="ingeste_maggy_02_conversation", flagName::String="maggy_02_talked", spriteId::String="magolor")

const placements = Ahorn.PlacementDict(
    "NPC02_Maggy" => Ahorn.EntityPlacement(NPC02_Maggy)
)

function Ahorn.selection(entity::NPC02_Maggy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC02_Maggy, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/magolor/idle00", entity.x, entity.y)
end

end
