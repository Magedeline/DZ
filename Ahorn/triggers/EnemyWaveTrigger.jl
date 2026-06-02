module EnemyWaveTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/EnemyWaveTrigger" EnemyWaveTrigger(x::Integer, y::Integer, enemiesPerWave::Integer=3, height::Integer=32, spawnDelay::Number=2.0, waveCount::Integer=3, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "EnemyWaveTrigger" => Ahorn.EntityPlacement(EnemyWaveTrigger)
)

function Ahorn.selection(entity::EnemyWaveTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::EnemyWaveTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
