module EnergyBarrier

using ..Ahorn, Maple

@mapdef Entity "DZ/EnergyBarrier" EnergyBarrier(x::Integer, y::Integer, barrierType::String="normal", color::String="00ffff", flagName::String="", flickerRate::Number=0.0, isActive::Bool=true, requiresPower::Bool=false, strength::Number=5.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(EnergyBarrier),
    "damage" => Ahorn.EntityPlacement(EnergyBarrier),
    "one_way" => Ahorn.EntityPlacement(EnergyBarrier)
)

function Ahorn.selection(entity::EnergyBarrier)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::EnergyBarrier, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/IngesteHelper/energy_barrier", entity.x, entity.y)
end

# Nodes: min=0, max=1
# Basic node rendering not implemented in auto-generated plugin

end
