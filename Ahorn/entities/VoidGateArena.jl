module VoidGateArena

using ..Ahorn, Maple

@mapdef Entity "DZ/VoidGateArena" VoidGateArena(x::Integer, y::Integer, completionFlag::String="void_gate_arena_complete", enemiesPerWave::Integer=2, requiredKills::Integer=8, spawnBoss::Bool=false, totalWaves::Integer=4)

const placements = Ahorn.PlacementDict(
    "void_gate_arena_easy" => Ahorn.EntityPlacement(VoidGateArena),
    "void_gate_arena_normal" => Ahorn.EntityPlacement(VoidGateArena),
    "void_gate_arena_hard" => Ahorn.EntityPlacement(VoidGateArena)
)

function Ahorn.selection(entity::VoidGateArena)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::VoidGateArena, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entity", entity.x, entity.y)
end

end
