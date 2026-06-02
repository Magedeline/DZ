module CompanionSummonTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/CompanionSummonTrigger" CompanionSummonTrigger(x::Integer, y::Integer, companionType::String="Bandana_Dee", height::Integer=16, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "CompanionSummonTrigger" => Ahorn.EntityPlacement(CompanionSummonTrigger)
)

function Ahorn.selection(entity::CompanionSummonTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CompanionSummonTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
