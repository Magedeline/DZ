module DXLaserGate

using ..Ahorn, Maple

@mapdef Entity "DZ/DXLaserGate" DXLaserGate(x::Integer, y::Integer, color::String="FF0000", offTime::Number=1.0, onTime::Number=2.0, startOn::Bool=true)

const placements = Ahorn.PlacementDict(
    "DXLaserGate" => Ahorn.EntityPlacement(DXLaserGate),
    "DXLaserGate_Fast" => Ahorn.EntityPlacement(DXLaserGate),
    "DXLaserGate_Slow" => Ahorn.EntityPlacement(DXLaserGate)
)

function Ahorn.selection(entity::DXLaserGate)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DXLaserGate, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=1, max=1
# Basic node rendering not implemented in auto-generated plugin

end
