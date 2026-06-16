module ReflectionTentacles

using ..Ahorn, Maple

@mapdef Entity "DZ/ReflectionTentacles" ReflectionTentacles(x::Integer, y::Integer, Player::Bool=true, aggressive::Bool=false, attackDistance::Number=32.0, color::String="8844ff", flagName::String="", retreatDistance::Number=64.0, speed::Number=80.0, tentacleCount::Integer=3)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(ReflectionTentacles),
    "aggressive" => Ahorn.EntityPlacement(ReflectionTentacles),
    "defensive" => Ahorn.EntityPlacement(ReflectionTentacles)
)

function Ahorn.selection(entity::ReflectionTentacles)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ReflectionTentacles, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/IngesteHelper/reflection_tentacles", entity.x, entity.y)
end

end
