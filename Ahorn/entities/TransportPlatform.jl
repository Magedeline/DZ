module TransportPlatform

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/TransportPlatform" TransportPlatform(x::Integer, y::Integer, height::Integer=8, isActive::Bool=true, moveSpeed::Number=60.0, platformType::String="stone", waitTime::Number=2.0, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "TransportPlatform" => Ahorn.EntityPlacement(TransportPlatform),
    "wooden" => Ahorn.EntityPlacement(TransportPlatform),
    "magical" => Ahorn.EntityPlacement(TransportPlatform)
)

function Ahorn.selection(entity::TransportPlatform)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TransportPlatform, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/moveBlock/base", entity.x, entity.y)
end

end
