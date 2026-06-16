module IngesteInteractTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/IngesteInteractTrigger" IngesteInteractTrigger(x::Integer, y::Integer, event::String="ch2_poem", event_2::String="ch3_diary", event_3::String="ch3_guestbook", height::Integer=16, onInteract::String="", onlyOnce::Bool=false, prompt::String="Press {button} to interact", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "DZ/IngesteInteractTrigger" => Ahorn.EntityPlacement(IngesteInteractTrigger)
)

function Ahorn.selection(entity::IngesteInteractTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::IngesteInteractTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
