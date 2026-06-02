module DetachPinkPlatberryTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/DetachPinkPlatberryTrigger" DetachPinkPlatberryTrigger(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "DetachPinkPlatberryTrigger" => Ahorn.EntityPlacement(DetachPinkPlatberryTrigger)
)

function Ahorn.selection(entity::DetachPinkPlatberryTrigger)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DetachPinkPlatberryTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=1, max=1
# Basic node rendering not implemented in auto-generated plugin

end
