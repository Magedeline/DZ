module Hole

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/Hole" Hole(x::Integer, y::Integer, Player::Bool=false, affectsEntities::Bool=true, holeType::String="black", radius::Number=64.0, soundEffect::String="event:/game/general/thing_booped", spawnParticles::Bool=true, strength::Number=5.0, teleportDestination::String="")

const placements = Ahorn.PlacementDict(
    "black_hole" => Ahorn.EntityPlacement(Hole),
    "white_hole" => Ahorn.EntityPlacement(Hole),
    "void_hole" => Ahorn.EntityPlacement(Hole),
    "portal_hole" => Ahorn.EntityPlacement(Hole)
)

function Ahorn.selection(entity::Hole)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Hole, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/IngesteHelper/hole", entity.x, entity.y)
end

end
