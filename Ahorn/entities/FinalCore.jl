module FinalCore

using ..Ahorn, Maple

@mapdef Entity "DZ/FinalCore" FinalCore(x::Integer, y::Integer, attackInterval::Number=2.0, health::Integer=10)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(FinalCore)
)

function Ahorn.selection(entity::FinalCore)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FinalCore, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
