module NPC_FinalRemix_AsrielAndGranny

using ..Ahorn, Maple

@mapdef Entity "DZ/NPC_FinalRemix_AsrielAndGranny" NPC_FinalRemix_AsrielAndGranny(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "NPC_FinalRemix_AsrielAndGranny" => Ahorn.EntityPlacement(NPC_FinalRemix_AsrielAndGranny)
)

function Ahorn.selection(entity::NPC_FinalRemix_AsrielAndGranny)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC_FinalRemix_AsrielAndGranny, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/asriel/idle00", entity.x, entity.y)
end

end
