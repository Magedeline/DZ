module NPC08_Maggy_Ending

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/NPC08_Maggy_Ending" NPC08_Maggy_Ending(x::Integer, y::Integer, dialogKey::String="ingeste_maggy_08_ending", flagName::String="maggy_08_ending", spriteId::String="magolor")

const placements = Ahorn.PlacementDict(
    "NPC08_Maggy_Ending" => Ahorn.EntityPlacement(NPC08_Maggy_Ending)
)

function Ahorn.selection(entity::NPC08_Maggy_Ending)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC08_Maggy_Ending, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/magolor/idle00", entity.x, entity.y)
end

end
