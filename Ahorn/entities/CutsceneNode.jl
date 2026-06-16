module CutsceneNode

using ..Ahorn, Maple

@mapdef Entity "DZ/CutsceneNode" CutsceneNode(x::Integer, y::Integer, nodeName::String="Kglobal::Player_skip")

const placements = Ahorn.PlacementDict(
    "cutscene_node" => Ahorn.EntityPlacement(CutsceneNode)
)

function Ahorn.selection(entity::CutsceneNode)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CutsceneNode, room::Maple.Room)
    Ahorn.drawSprite(ctx, "@Internal@/cutscene_node", entity.x, entity.y)
end

end
