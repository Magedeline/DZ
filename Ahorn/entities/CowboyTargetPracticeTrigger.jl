module CowboyTargetPracticeTrigger

using ..Ahorn, Maple

@mapdef Entity "DZ/CowboyTargetPracticeTrigger" CowboyTargetPracticeTrigger(x::Integer, y::Integer, flagToCheck::String="", height::Integer=16, movingTargets::Bool=false, practiceType::String="A", requiredTargets::Integer=10, timeLimit::Number=60.0, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "trigger" => Ahorn.EntityPlacement(CowboyTargetPracticeTrigger)
)

function Ahorn.selection(entity::CowboyTargetPracticeTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CowboyTargetPracticeTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
