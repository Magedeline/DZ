module WhispyWoodsBoss

using ..Ahorn, Maple

@mapdef Entity "DZ/WhispyWoodsBoss" WhispyWoodsBoss(x::Integer, y::Integer, attackSequence::String="", cameraLockY::Bool=true, cameraPastY::Number=120.0, dialog::Bool=false, patternIndex::Integer=1, startHit::Bool=false)

const placements = Ahorn.PlacementDict(
    "whispy_woods_boss" => Ahorn.EntityPlacement(WhispyWoodsBoss)
)

function Ahorn.selection(entity::WhispyWoodsBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WhispyWoodsBoss, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/DesoloZantas/whispy_woods/idle00", entity.x, entity.y)
end

# Nodes: min=1, max=20
# Basic node rendering not implemented in auto-generated plugin

end
