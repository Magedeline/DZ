module BossDifficultySelector

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/BossDifficultySelector" BossDifficultySelector(x::Integer, y::Integer, bossId::String="", defaultDifficulty::String="normal", showBeforeBoss::Bool=true)

const placements = Ahorn.PlacementDict(
    "Boss Difficulty Selector" => Ahorn.EntityPlacement(BossDifficultySelector)
)

function Ahorn.selection(entity::BossDifficultySelector)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BossDifficultySelector, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
