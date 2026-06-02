module RalseiBoost

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/RalseiBoost" RalseiBoost(x::Integer, y::Integer, ambientParticle1::String="f78ae7", ambientParticle2::String="ffccf7", canSkip::Bool=false, cutsceneBird::Bool=true, cutsceneTeleport::String="", goldenTeleport::String="", lockCamera::Bool=false, moveColor::String="ff6def", moveImage::String="", moveParticleColor::String="e0a8d8", preLaunchDialog::String="")

const placements = Ahorn.PlacementDict(
    "MaggyHelper/RalseiBoost" => Ahorn.EntityPlacement(RalseiBoost)
)

function Ahorn.selection(entity::RalseiBoost)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RalseiBoost, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/Maggy/DesoloZantas/ralseiboost/idle00", entity.x, entity.y)
end

end
