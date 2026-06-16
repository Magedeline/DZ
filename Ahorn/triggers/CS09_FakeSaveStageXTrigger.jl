module CS09_FakeSaveStageXTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/CS09_FakeSaveStageXTrigger" CS09_FakeSaveStageXTrigger(x::Integer, y::Integer, height::Integer=16, playerOnly::Bool=true, stage::String="stageA", triggerOnce::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "CS09_FakeSaveStageXTrigger" => Ahorn.EntityPlacement(CS09_FakeSaveStageXTrigger)
)

function Ahorn.selection(entity::CS09_FakeSaveStageXTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CS09_FakeSaveStageXTrigger, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entityTrigger", entity.x, entity.y)
end

end
