module HologramProjector

using ..Ahorn, Maple

@mapdef Entity "DZ/HologramProjector" HologramProjector(x::Integer, y::Integer, displayTime::Number=5.0, message::String="Hello!")

const placements = Ahorn.PlacementDict(
    "HologramProjector" => Ahorn.EntityPlacement(HologramProjector)
)

function Ahorn.selection(entity::HologramProjector)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HologramProjector, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
