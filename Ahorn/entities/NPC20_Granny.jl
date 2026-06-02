module NPC20_Granny

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/NPC20_Granny" NPC20_Granny(x::Integer, y::Integer, dialogKey::String="ingeste_granny_20_final", flagName::String="granny_20_final", spriteId::String="oldlady", talking::Bool=false)

const placements = Ahorn.PlacementDict(
    "NPC20_Granny" => Ahorn.EntityPlacement(NPC20_Granny)
)

function Ahorn.selection(entity::NPC20_Granny)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC20_Granny, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/oldlady/idle00", entity.x, entity.y)
end

end
