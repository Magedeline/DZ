module metoriteblock

using ..Ahorn, Maple

@mapdef Entity "DZ/metoriteblock" metoriteblock(x::Integer, y::Integer, destroyOnImpact::Bool=true, explosionRadius::Number=32.0, explosive::Bool=false, fallSpeed::Number=200.0, fireTrail::Bool=true, height::Integer=16, impactDamage::Bool=false, respawnTime::Number=10.0, shakeOnImpact::Bool=true, tiletype::String="7", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(metoriteblock),
    "explosive" => Ahorn.EntityPlacement(metoriteblock),
    "fast" => Ahorn.EntityPlacement(metoriteblock)
)

function Ahorn.selection(entity::metoriteblock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::metoriteblock, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/IngesteHelper/meteorite_block", entity.x, entity.y)
end

end
