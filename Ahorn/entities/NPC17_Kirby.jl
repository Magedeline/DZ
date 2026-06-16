module NPC17_Kirby

using ..Ahorn, Maple

@mapdef Entity "DZ/NPC17_Kirby" NPC17_Kirby(x::Integer, y::Integer, dialogKey::String="ingeste_kirby_17_ending", flagName::String="kirby_17_ending", spriteId::String="kirby", talking::Bool=false)

const placements = Ahorn.PlacementDict(
    "NPC17_Kirby" => Ahorn.EntityPlacement(NPC17_Kirby)
)

function Ahorn.selection(entity::NPC17_Kirby)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC17_Kirby, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/kirby/idle00", entity.x, entity.y)
end

end
