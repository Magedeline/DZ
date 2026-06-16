module HeartDoorMod

using ..Ahorn, Maple

@mapdef Entity "DZ/HeartDoorMod" HeartDoorMod(x::Integer, y::Integer, requires::Integer=0, startHidden::Bool=false, width::Integer=40)

const placements = Ahorn.PlacementDict(
    "heart_door" => Ahorn.EntityPlacement(HeartDoorMod),
    "heart_door_hidden" => Ahorn.EntityPlacement(HeartDoorMod)
)

function Ahorn.selection(entity::HeartDoorMod)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HeartDoorMod, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=0, max=1
# Basic node rendering not implemented in auto-generated plugin

end
