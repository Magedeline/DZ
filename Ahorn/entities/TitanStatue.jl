module TitanStatue

using ..Ahorn, Maple

@mapdef Entity "DZ/TitanStatue" TitanStatue(x::Integer, y::Integer, canAwaken::Bool=false, health::Integer=5, isAnimated::Bool=false)

const placements = Ahorn.PlacementDict(
    "inactive" => Ahorn.EntityPlacement(TitanStatue),
    "can_awaken" => Ahorn.EntityPlacement(TitanStatue),
    "animated" => Ahorn.EntityPlacement(TitanStatue)
)

function Ahorn.selection(entity::TitanStatue)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TitanStatue, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
