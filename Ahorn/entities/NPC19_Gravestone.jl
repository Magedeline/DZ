module NPC19_Gravestone

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/NPC19_Gravestone" NPC19_Gravestone(x::Integer, y::Integer, dialogKey::String="CH19_GRAVESTONE", flagName::String="maddy_gravestone", spriteId::String="maddygrave00")

const placements = Ahorn.PlacementDict(
    "NPC19_Gravestone" => Ahorn.EntityPlacement(NPC19_Gravestone)
)

function Ahorn.selection(entity::NPC19_Gravestone)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC19_Gravestone, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/gravestones/maddygrave00", entity.x, entity.y)
end

end
