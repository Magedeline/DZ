module JudgmentBell

using ..Ahorn, Maple

@mapdef Entity "DZ/JudgmentBell" JudgmentBell(x::Integer, y::Integer, PlayerRing::Bool=true, cooldownTime::Number=2.0, maxRings::Integer=3, shockwaveRadius::Integer=300, shockwaveSpeed::Integer=200)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(JudgmentBell)
)

function Ahorn.selection(entity::JudgmentBell)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::JudgmentBell, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
