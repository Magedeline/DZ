module InfernoMirror

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/InfernoMirror" InfernoMirror(x::Integer, y::Integer, height::Integer=32, reflectX::Number=0.0, reflectY::Number=0.0, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(InfernoMirror)
)

function Ahorn.selection(entity::InfernoMirror)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::InfernoMirror, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
