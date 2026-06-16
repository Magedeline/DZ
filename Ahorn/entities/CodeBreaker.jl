module CodeBreaker

using ..Ahorn, Maple

@mapdef Entity "DZ/CodeBreaker" CodeBreaker(x::Integer, y::Integer, code::String="1234", inputTimeout::Number=5.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(CodeBreaker)
)

function Ahorn.selection(entity::CodeBreaker)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CodeBreaker, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
