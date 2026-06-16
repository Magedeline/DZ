module HotHead

using ..Ahorn, Maple

@mapdef Entity "DZ/HotHead" HotHead(x::Integer, y::Integer, canBeInhaled::Bool=true, health::Integer=1, moveSpeed::Number=25.0)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(HotHead)
)

function Ahorn.selection(entity::HotHead)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HotHead, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
