module GhostlyEcho

using ..Ahorn, Maple

@mapdef Entity "DZ/GhostlyEcho" GhostlyEcho(x::Integer, y::Integer, alpha::Number=0.6, behavior::String="Mirror", fadeTime::Number=2.0, isDangerous::Bool=true, isSolid::Bool=false, mirrorDelay::Number=0.5)

const placements = Ahorn.PlacementDict(
    "mirror" => Ahorn.EntityPlacement(GhostlyEcho),
    "chase" => Ahorn.EntityPlacement(GhostlyEcho)
)

function Ahorn.selection(entity::GhostlyEcho)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GhostlyEcho, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
