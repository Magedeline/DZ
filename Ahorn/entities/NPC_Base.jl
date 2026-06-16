module NPC_Base

using ..Ahorn, Maple

@mapdef Entity "DZ/NPC_Base" NPC_Base(x::Integer, y::Integer, npcName::String="NPC")

const placements = Ahorn.PlacementDict(
    "npc_base" => Ahorn.EntityPlacement(NPC_Base)
)

function Ahorn.selection(entity::NPC_Base)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC_Base, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/Kglobal::Player/idle00", entity.x, entity.y)
end

end
