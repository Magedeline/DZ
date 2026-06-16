module FlingBirdMod

using ..Ahorn, Maple

@mapdef Entity "DZ/FlingBirdMod" FlingBirdMod(x::Integer, y::Integer, waiting::Bool=false)

const placements = Ahorn.PlacementDict(
    "fling_bird" => Ahorn.EntityPlacement(FlingBirdMod)
)

function Ahorn.selection(entity::FlingBirdMod)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FlingBirdMod, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/bird/Hover04", entity.x, entity.y)
end

end
