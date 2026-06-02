module TimeCapsule

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/TimeCapsule" TimeCapsule(x::Integer, y::Integer, radius::Number=80.0, slowFactor::Number=0.5)

const placements = Ahorn.PlacementDict(
    "TimeCapsule" => Ahorn.EntityPlacement(TimeCapsule),
    "TimeCapsule_strong_slow" => Ahorn.EntityPlacement(TimeCapsule)
)

function Ahorn.selection(entity::TimeCapsule)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TimeCapsule, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
