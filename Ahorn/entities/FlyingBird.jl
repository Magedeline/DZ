module FlyingBird

using ..Ahorn, Maple

@mapdef Entity "DZ/FlyingBird" FlyingBird(x::Integer, y::Integer, disableFlapSfx::Bool=false, emitFeathers::Bool=false, loopPath::Bool=false, speed::Number=60.0)

const placements = Ahorn.PlacementDict(
    "flying_bird" => Ahorn.EntityPlacement(FlyingBird),
    "flying_bird_looping" => Ahorn.EntityPlacement(FlyingBird)
)

function Ahorn.selection(entity::FlyingBird)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FlyingBird, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/bird/Hover04", entity.x, entity.y)
end

end
