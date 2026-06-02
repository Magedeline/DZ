module oshirodoor

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/oshirodoor" oshirodoor(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "oshirodoor" => Ahorn.EntityPlacement(oshirodoor)
)

function Ahorn.selection(entity::oshirodoor)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::oshirodoor, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
