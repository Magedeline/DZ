module MorphoKnightClone

using ..Ahorn, Maple

@mapdef Entity "DZ/MorphoKnightClone" MorphoKnightClone(x::Integer, y::Integer, attackCooldown::Number=1.05, bossMusic::String="event:/pusheen/music/lvl15/morpho_knight", chargeAnimationPath::String="vortex_summon", dashSpeed::Number=320.0, health::Integer=10, idleAnimationPath::String="swords", moveAnimationPath::String="swords", moveSpeed::Number=142.0, orbitRadius::Number=66.0, playMusicOnStart::Bool=false, slashAnimationPath::String="double_side_slash", spritePath::String="characters/els_true_final_boss/Maggy/DesoloZantas/siamo_zero_morpho_knight_fake/", warpAnimationPath::String="vortex_strike")

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(MorphoKnightClone)
)

function Ahorn.selection(entity::MorphoKnightClone)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MorphoKnightClone, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
