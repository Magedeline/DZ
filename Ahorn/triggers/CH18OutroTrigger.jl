module CH18OutroTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/CH18OutroTrigger" CH18OutroTrigger(x::Integer, y::Integer, height::Integer=32, triggerOnce::Bool=true, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "ch18_outro_trigger" => Ahorn.EntityPlacement(CH18OutroTrigger)
)

function Ahorn.selection(entity::CH18OutroTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CH18OutroTrigger, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entityTrigger", entity.x, entity.y)
end

end
