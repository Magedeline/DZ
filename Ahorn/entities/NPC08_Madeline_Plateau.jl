module NPC08_Madeline_Plateau

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/NPC08_Madeline_Plateau" NPC08_Madeline_Plateau(x::Integer, y::Integer, dialogKey::String="ingeste_madeline_08_plateau", flagName::String="madeline_08_plateau", spriteId::String="madeline")

const placements = Ahorn.PlacementDict(
    "NPC08_Madeline_Plateau" => Ahorn.EntityPlacement(NPC08_Madeline_Plateau)
)

function Ahorn.selection(entity::NPC08_Madeline_Plateau)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC08_Madeline_Plateau, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/madeline/idle00", entity.x, entity.y)
end

end
