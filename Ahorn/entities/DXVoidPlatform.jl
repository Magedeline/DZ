module DXVoidPlatform

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/DXVoidPlatform" DXVoidPlatform(x::Integer, y::Integer, phaseDuration::Number=1.0, phaseInterval::Number=2.0, phaseOffset::Number=0.0, width::Integer=48)

const placements = Ahorn.PlacementDict(
    "DXVoidPlatform" => Ahorn.EntityPlacement(DXVoidPlatform)
)

function Ahorn.selection(entity::DXVoidPlatform)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DXVoidPlatform, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
