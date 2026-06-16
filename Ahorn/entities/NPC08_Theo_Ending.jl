module NPC08_Theo_Ending

using ..Ahorn, Maple

@mapdef Entity "DZ/NPC08_Theo_Ending" NPC08_Theo_Ending(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "NPC08_Theo_Ending" => Ahorn.EntityPlacement(NPC08_Theo_Ending)
)

function Ahorn.selection(entity::NPC08_Theo_Ending)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC08_Theo_Ending, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/theo/theo00", entity.x, entity.y)
end

end
