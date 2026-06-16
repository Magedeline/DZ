module Torch

using ..Ahorn, Maple

@mapdef Entity "DZ/Torch" Torch(x::Integer, y::Integer, isLit::Bool=true, lightRadius::Number=64.0, torchType::String="wall")

const placements = Ahorn.PlacementDict(
    "Torch" => Ahorn.EntityPlacement(Torch),
    "floor" => Ahorn.EntityPlacement(Torch),
    "magical" => Ahorn.EntityPlacement(Torch)
)

function Ahorn.selection(entity::Torch)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Torch, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/torch/magical_torch", entity.x, entity.y)
end

end
