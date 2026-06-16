module PortalDoor

using ..Ahorn, Maple

@mapdef Entity "DZ/PortalDoor" PortalDoor(x::Integer, y::Integer, portalId::String="portal_A")

const placements = Ahorn.PlacementDict(
    "portal_A" => Ahorn.EntityPlacement(PortalDoor),
    "portal_B" => Ahorn.EntityPlacement(PortalDoor)
)

function Ahorn.selection(entity::PortalDoor)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PortalDoor, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/Maggy/DesoloZantas/door/door00", entity.x, entity.y)
end

# Nodes: min=0, max=1
# Basic node rendering not implemented in auto-generated plugin

end
