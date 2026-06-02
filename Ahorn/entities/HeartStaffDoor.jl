module HeartStaffDoor

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/HeartStaffDoor" HeartStaffDoor(x::Integer, y::Integer, doorId::String="", requires::Integer=7, startHidden::Bool=false, width::Integer=80)

const placements = Ahorn.PlacementDict(
    "heart_staff_door" => Ahorn.EntityPlacement(HeartStaffDoor),
    "heart_staff_door_hidden" => Ahorn.EntityPlacement(HeartStaffDoor),
    "heart_staff_door_partial" => Ahorn.EntityPlacement(HeartStaffDoor)
)

function Ahorn.selection(entity::HeartStaffDoor)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HeartStaffDoor, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=0, max=1
# Basic node rendering not implemented in auto-generated plugin

end
