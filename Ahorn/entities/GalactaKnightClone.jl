module GalactaKnightClone

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/GalactaKnightClone" GalactaKnightClone(x::Integer, y::Integer, attackCooldown::Number=1.15, bossMusic::String="event:/pusheen/music/lvl18/galacta_knight", chargeAnimationPath::String="awaken", dashSpeed::Number=340.0, health::Integer=12, idleAnimationPath::String="idle", moveAnimationPath::String="move", moveSpeed::Number=150.0, orbitRadius::Number=74.0, playMusicOnStart::Bool=false, slashAnimationPath::String="rapid_slash", spritePath::String="characters/els_true_final_boss/Maggy/DesoloZantas/siamo_zero_aeon_hero_fake/", warpAnimationPath::String="fly_start")

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(GalactaKnightClone)
)

function Ahorn.selection(entity::GalactaKnightClone)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GalactaKnightClone, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
