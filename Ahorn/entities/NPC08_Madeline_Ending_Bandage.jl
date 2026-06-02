module NPC08_Madeline_Ending_Bandage

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/NPC08_Madeline_Ending_Bandage" NPC08_Madeline_Ending_Bandage(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "NPC08_Madeline_Ending_Bandage" => Ahorn.EntityPlacement(NPC08_Madeline_Ending_Bandage)
)

function Ahorn.selection(entity::NPC08_Madeline_Ending_Bandage)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC08_Madeline_Ending_Bandage, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/madeline_bandage/idle00", entity.x, entity.y)
end

end
