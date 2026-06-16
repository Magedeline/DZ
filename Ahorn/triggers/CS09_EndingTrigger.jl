module CS09_EndingTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/CS09_EndingTrigger" CS09_EndingTrigger(x::Integer, y::Integer, height::Integer=16, nextLevel::String="", requireTrapComplete::Bool=true, skipCredits::Bool=false, skipMessage::Bool=false, triggerOnce::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "full_ending" => Ahorn.EntityPlacement(CS09_EndingTrigger),
    "credits_only" => Ahorn.EntityPlacement(CS09_EndingTrigger),
    "message_only" => Ahorn.EntityPlacement(CS09_EndingTrigger),
    "area_complete_only" => Ahorn.EntityPlacement(CS09_EndingTrigger),
    "after_trap_full" => Ahorn.EntityPlacement(CS09_EndingTrigger)
)

function Ahorn.selection(entity::CS09_EndingTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CS09_EndingTrigger, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entityTrigger", entity.x, entity.y)
end

end
