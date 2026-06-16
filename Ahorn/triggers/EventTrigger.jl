module EventTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/EventTrigger" EventTrigger(x::Integer, y::Integer, event::String="", height::Integer=16, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(EventTrigger),
    "chapter_intro" => Ahorn.EntityPlacement(EventTrigger),
    "boss_intro" => Ahorn.EntityPlacement(EventTrigger),
    "boss_mid" => Ahorn.EntityPlacement(EventTrigger),
    "boss_end" => Ahorn.EntityPlacement(EventTrigger),
    "ending" => Ahorn.EntityPlacement(EventTrigger),
    "credits" => Ahorn.EntityPlacement(EventTrigger)
)

function Ahorn.selection(entity::EventTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::EventTrigger, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entityTrigger", entity.x, entity.y)
end

end
