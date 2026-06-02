module CS09_UntilNextTimeCreditTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/CS09_UntilNextTimeCreditTrigger" CS09_UntilNextTimeCreditTrigger(x::Integer, y::Integer, height::Integer=16, requireFlag::Bool=false, requiredFlag::String="", triggerOnce::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "CS09_UntilNextTimeCreditTrigger" => Ahorn.EntityPlacement(CS09_UntilNextTimeCreditTrigger),
    "after_trap" => Ahorn.EntityPlacement(CS09_UntilNextTimeCreditTrigger),
    "end_game" => Ahorn.EntityPlacement(CS09_UntilNextTimeCreditTrigger)
)

function Ahorn.selection(entity::CS09_UntilNextTimeCreditTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CS09_UntilNextTimeCreditTrigger, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entityTrigger", entity.x, entity.y)
end

end
