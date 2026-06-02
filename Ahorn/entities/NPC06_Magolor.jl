module NPC06_Magolor

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/NPC06_Magolor" NPC06_Magolor(x::Integer, y::Integer, dialogue::String="NPC06_MAGOLOR_DEFAULT", floating::Bool=true)

const placements = Ahorn.PlacementDict(
    "NPC06_Magolor" => Ahorn.EntityPlacement(NPC06_Magolor)
)

function Ahorn.selection(entity::NPC06_Magolor)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC06_Magolor, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/magolor/idle00", entity.x, entity.y)
end

end
