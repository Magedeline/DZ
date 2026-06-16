module CharaMirror

using ..Ahorn, Maple

@mapdef Entity "DZ/CharaMirror" CharaMirror(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "CharaMirror" => Ahorn.EntityPlacement(CharaMirror)
)

function Ahorn.selection(entity::CharaMirror)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CharaMirror, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
