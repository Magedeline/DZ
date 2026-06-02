module JumpThru

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/JumpThru" JumpThru(x::Integer, y::Integer, Player::Bool=false, attachToSolids::Bool=false, height::Integer=8, moveSpeed::Number=0.0, overrideTexture::String="", sinkAmount::Number=0.0, surfaceIndex::Integer=-1, texture::String="wood", width::Integer=24)

const placements = Ahorn.PlacementDict(
    "wood" => Ahorn.EntityPlacement(JumpThru),
    "stone" => Ahorn.EntityPlacement(JumpThru),
    "moving" => Ahorn.EntityPlacement(JumpThru),
    "cloud" => Ahorn.EntityPlacement(JumpThru)
)

function Ahorn.selection(entity::JumpThru)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::JumpThru, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/IngesteHelper/jump_thru", entity.x, entity.y)
end

end
