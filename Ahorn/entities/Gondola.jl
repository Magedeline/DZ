module Gondola

using ..Ahorn, Maple

@mapdef Entity "DZ/Gondola" Gondola(x::Integer, y::Integer, active::Bool=true, triggerCutscene::Bool=false)

const placements = Ahorn.PlacementDict(
    "gondola_mod" => Ahorn.EntityPlacement(Gondola)
)

function Ahorn.selection(entity::Gondola)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Gondola, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=1, max=1
# Basic node rendering not implemented in auto-generated plugin

end
