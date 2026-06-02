module CS09_FakeSavePointTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/CS09_FakeSavePointTrigger" CS09_FakeSavePointTrigger(x::Integer, y::Integer, height::Integer=16, playerOnly::Bool=true, specificStage::String="auto", triggerOnce::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "auto_stage" => Ahorn.EntityPlacement(CS09_FakeSavePointTrigger),
    "stage_a" => Ahorn.EntityPlacement(CS09_FakeSavePointTrigger),
    "stage_b" => Ahorn.EntityPlacement(CS09_FakeSavePointTrigger),
    "stage_c" => Ahorn.EntityPlacement(CS09_FakeSavePointTrigger),
    "stage_d" => Ahorn.EntityPlacement(CS09_FakeSavePointTrigger),
    "stage_e" => Ahorn.EntityPlacement(CS09_FakeSavePointTrigger),
    "pretrap" => Ahorn.EntityPlacement(CS09_FakeSavePointTrigger),
    "trap" => Ahorn.EntityPlacement(CS09_FakeSavePointTrigger),
    "madeline_freakout" => Ahorn.EntityPlacement(CS09_FakeSavePointTrigger)
)

function Ahorn.selection(entity::CS09_FakeSavePointTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CS09_FakeSavePointTrigger, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entityTrigger", entity.x, entity.y)
end

end
