module CharaBoss

using ..Ahorn, Maple

@mapdef Entity "DZ/CharaBoss" CharaBoss(x::Integer, y::Integer, cameraLockY::Bool=true, cameraPastY::Number=120.0, dialog::Bool=false, patternIndex::Integer=0, startHit::Bool=false)

const placements = Ahorn.PlacementDict(
    "chara_boss" => Ahorn.EntityPlacement(CharaBoss)
)

function Ahorn.selection(entity::CharaBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CharaBoss, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/charaBoss/boss00", entity.x, entity.y)
end

# Nodes: min=1, max=20
# Basic node rendering not implemented in auto-generated plugin

end
