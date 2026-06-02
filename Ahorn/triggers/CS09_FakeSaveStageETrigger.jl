module CS09_FakeSaveStageETrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/CS09_FakeSaveStageETrigger" CS09_FakeSaveStageETrigger(x::Integer, y::Integer, height::Integer=16, playerOnly::Bool=true, triggerOnce::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "CS09_FakeSaveStageETrigger" => Ahorn.EntityPlacement(CS09_FakeSaveStageETrigger)
)

function Ahorn.selection(entity::CS09_FakeSaveStageETrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CS09_FakeSaveStageETrigger, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entityTrigger", entity.x, entity.y)
end

end
