module GondolaMaggy

using ..Ahorn, Maple

@mapdef Entity "DZs/GondolaMaggy" GondolaMaggy(x::Integer, y::Integer, active::Bool=true)

const placements = Ahorn.PlacementDict(
    "gondolamaggy" => Ahorn.EntityPlacement(GondolaMaggy)
)

function Ahorn.selection(entity::GondolaMaggy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GondolaMaggy, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=1, max=1
# Basic node rendering not implemented in auto-generated plugin

end
