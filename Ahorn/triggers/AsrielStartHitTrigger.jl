module AsrielStartHitTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/AsrielStartHitTrigger" AsrielStartHitTrigger(x::Integer, y::Integer, height::Integer=16, moveAsriel::Bool=false, moveSpeed::Number=300.0, triggerOnce::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "AsrielStartHitTrigger" => Ahorn.EntityPlacement(AsrielStartHitTrigger),
    "with_movement" => Ahorn.EntityPlacement(AsrielStartHitTrigger)
)

function Ahorn.selection(entity::AsrielStartHitTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AsrielStartHitTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=0, max=1
# Basic node rendering not implemented in auto-generated plugin

end
