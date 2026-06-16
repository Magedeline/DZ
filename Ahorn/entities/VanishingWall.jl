module VanishingWall

using ..Ahorn, Maple

@mapdef Entity "DZ/VanishingWall" VanishingWall(x::Integer, y::Integer, activateOnDash::Bool=false, activateOnTouch::Bool=true, activationTime::Number=0.5, duration::Number=3.0, flagName::String="", height::Integer=16, persistent::Bool=false, respawnTime::Number=5.0, tiletype::String="3", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "VanishingWall" => Ahorn.EntityPlacement(VanishingWall),
    "dash_activated" => Ahorn.EntityPlacement(VanishingWall)
)

function Ahorn.selection(entity::VanishingWall)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::VanishingWall, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/vanishing_wall", entity.x, entity.y)
end

end
