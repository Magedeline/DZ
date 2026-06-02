module EnemySpawnTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/EnemySpawnTrigger" EnemySpawnTrigger(x::Integer, y::Integer, count::Integer=1, enemyType::String="WaddleDee", height::Integer=16, respawn::Bool=false, spawnDelay::Number=0.0, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "waddle_dee" => Ahorn.EntityPlacement(EnemySpawnTrigger),
    "waddle_doo" => Ahorn.EntityPlacement(EnemySpawnTrigger),
    "gordo" => Ahorn.EntityPlacement(EnemySpawnTrigger),
    "scarfy" => Ahorn.EntityPlacement(EnemySpawnTrigger)
)

function Ahorn.selection(entity::EnemySpawnTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::EnemySpawnTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
