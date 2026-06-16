module DarkernerFountain

using ..Ahorn, Maple

@mapdef Entity "DZ/DarkernerFountain" DarkernerFountain(x::Integer, y::Integer, Player::Bool=true, activationRadius::Integer=64, autoActivate::Bool=false, duration::Number=10.0, fountainType::String="Chaos", height::Integer=48, intensity::Number=1.5, particleCount::Integer=50, persistentEffect::Bool=false, requiresFlag::String="", soundEffect::String="event:/game/general/thing_booped", width::Integer=32)

const placements = Ahorn.PlacementDict(
    "chaos_fountain" => Ahorn.EntityPlacement(DarkernerFountain),
    "pure_fountain" => Ahorn.EntityPlacement(DarkernerFountain),
    "shadow_fountain" => Ahorn.EntityPlacement(DarkernerFountain)
)

function Ahorn.selection(entity::DarkernerFountain)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DarkernerFountain, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/fountain_darkener", entity.x, entity.y)
end

end
