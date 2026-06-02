module AreaCompleteTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/AreaCompleteTrigger" AreaCompleteTrigger(x::Integer, y::Integer, hasGoldenStrawberry::Bool=false, hasPinkPlatinumBerry::Bool=false, height::Integer=16, nextLevel::String="", skipCredits::Bool=false, triggerOnce::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "AreaCompleteTrigger" => Ahorn.EntityPlacement(AreaCompleteTrigger),
    "with_next_level" => Ahorn.EntityPlacement(AreaCompleteTrigger),
    "golden_ending" => Ahorn.EntityPlacement(AreaCompleteTrigger),
    "pink_platinum_ending" => Ahorn.EntityPlacement(AreaCompleteTrigger),
    "quick_transition" => Ahorn.EntityPlacement(AreaCompleteTrigger)
)

function Ahorn.selection(entity::AreaCompleteTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AreaCompleteTrigger, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entityTrigger", entity.x, entity.y)
end

end
