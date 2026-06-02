module NPC06_Theo

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/NPC06_Theo" NPC06_Theo(x::Integer, y::Integer, conversationStage::Integer=1)

const placements = Ahorn.PlacementDict(
    "NPC06_Theo" => Ahorn.EntityPlacement(NPC06_Theo)
)

function Ahorn.selection(entity::NPC06_Theo)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC06_Theo, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/theo/theo00", entity.x, entity.y)
end

end
