module NPC17_Oshiro

using ..Ahorn, Maple

@mapdef Entity "DZ/NPC17_Oshiro" NPC17_Oshiro(x::Integer, y::Integer, dialogKey::String="ingeste_oshiro_17_final", flagName::String="oshiro_17_final", spriteId::String="oshiro", talking::Bool=false)

const placements = Ahorn.PlacementDict(
    "NPC17_Oshiro" => Ahorn.EntityPlacement(NPC17_Oshiro)
)

function Ahorn.selection(entity::NPC17_Oshiro)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC17_Oshiro, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/oshiro/oshiro00", entity.x, entity.y)
end

end
