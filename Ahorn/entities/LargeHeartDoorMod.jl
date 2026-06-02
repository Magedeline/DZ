module LargeHeartDoorMod

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/LargeHeartDoorMod" LargeHeartDoorMod(x::Integer, y::Integer, requires::Integer=0, startHidden::Bool=false, width::Integer=64)

const placements = Ahorn.PlacementDict(
    "large_heart_door" => Ahorn.EntityPlacement(LargeHeartDoorMod),
    "large_heart_door_hidden" => Ahorn.EntityPlacement(LargeHeartDoorMod)
)

function Ahorn.selection(entity::LargeHeartDoorMod)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::LargeHeartDoorMod, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=0, max=1
# Basic node rendering not implemented in auto-generated plugin

end
