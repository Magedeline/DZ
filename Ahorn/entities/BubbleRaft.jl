module BubbleRaft

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/BubbleRaft" BubbleRaft(x::Integer, y::Integer, duration::Number=5.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(BubbleRaft),
    "fast" => Ahorn.EntityPlacement(BubbleRaft),
    "long_lasting" => Ahorn.EntityPlacement(BubbleRaft)
)

function Ahorn.selection(entity::BubbleRaft)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BubbleRaft, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
