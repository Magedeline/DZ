module DarkMirror

using ..Ahorn, Maple

@mapdef Entity "DZ/DarkMirror" DarkMirror(x::Integer, y::Integer, health::Integer=2, revealsSecret::Bool=false)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(DarkMirror),
    "secret" => Ahorn.EntityPlacement(DarkMirror)
)

function Ahorn.selection(entity::DarkMirror)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DarkMirror, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
