module RedLightning

using ..Ahorn, Maple

@mapdef Entity "DZ/RedLightning" RedLightning(x::Integer, y::Integer, Player::Bool=false, color::String="FF0000FF", duration::Number=2.0, height::Integer=64, intensity::Number=1.0, soundEffect::String="event:/game/general/lightning", width::Integer=32)

const placements = Ahorn.PlacementDict(
    "dramatic_effect" => Ahorn.EntityPlacement(RedLightning),
    "chase_effect" => Ahorn.EntityPlacement(RedLightning),
    "ambient_effect" => Ahorn.EntityPlacement(RedLightning)
)

function Ahorn.selection(entity::RedLightning)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RedLightning, room::Maple.Room)
    Ahorn.drawSprite(ctx, "effects/lightning_red", entity.x, entity.y)
end

end
