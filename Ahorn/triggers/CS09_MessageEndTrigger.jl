module CS09_MessageEndTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/CS09_MessageEndTrigger" CS09_MessageEndTrigger(x::Integer, y::Integer, height::Integer=16, requireFlag::Bool=false, requiredFlag::String="", triggerOnce::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "CS09_MessageEndTrigger" => Ahorn.EntityPlacement(CS09_MessageEndTrigger),
    "after_credits" => Ahorn.EntityPlacement(CS09_MessageEndTrigger),
    "after_trap" => Ahorn.EntityPlacement(CS09_MessageEndTrigger)
)

function Ahorn.selection(entity::CS09_MessageEndTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CS09_MessageEndTrigger, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entityTrigger", entity.x, entity.y)
end

end
