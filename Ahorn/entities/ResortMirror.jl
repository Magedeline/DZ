module ResortMirror

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/ResortMirror" ResortMirror(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "ResortMirror" => Ahorn.EntityPlacement(ResortMirror)
)

function Ahorn.selection(entity::ResortMirror)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ResortMirror, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
