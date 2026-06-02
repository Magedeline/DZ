module EnhancedPowerGenerator

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/EnhancedPowerGenerator" EnhancedPowerGenerator(x::Integer, y::Integer, currentFuel::Number=0.0, efficiency::Number=0.8, fuelType::String="none", generatorType::String="solar", isActive::Bool=true, maxFuelCapacity::Number=0.0, particleColor::String="ffff44", powerOutput::Number=25.0, requiresFuel::Bool=false, showParticles::Bool=true)

const placements = Ahorn.PlacementDict(
    "solar" => Ahorn.EntityPlacement(EnhancedPowerGenerator),
    "fusion" => Ahorn.EntityPlacement(EnhancedPowerGenerator),
    "magical" => Ahorn.EntityPlacement(EnhancedPowerGenerator)
)

function Ahorn.selection(entity::EnhancedPowerGenerator)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::EnhancedPowerGenerator, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/IngesteHelper/enhanced_power_generator", entity.x, entity.y)
end

end
