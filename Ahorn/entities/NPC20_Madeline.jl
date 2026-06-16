module NPC20_Madeline

using ..Ahorn, Maple

@mapdef Entity "DZ/NPC20_Madeline" NPC20_Madeline(x::Integer, y::Integer, dialogKey::String="ingeste_madeline_20_saved", flagName::String="madeline_20_saved", spriteId::String="madeline", talking::Bool=false)

const placements = Ahorn.PlacementDict(
    "NPC20_Madeline" => Ahorn.EntityPlacement(NPC20_Madeline)
)

function Ahorn.selection(entity::NPC20_Madeline)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC20_Madeline, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/madeline/idle00", entity.x, entity.y)
end

end
