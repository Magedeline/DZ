module WaterfallClimb

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/WaterfallClimb" WaterfallClimb(x::Integer, y::Integer, flowStrength::Integer=80, height::Integer=64, rushDuration::Number=2.0, rushInterval::Number=5.0, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(WaterfallClimb)
)

function Ahorn.selection(entity::WaterfallClimb)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WaterfallClimb, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
