module WaterThermos

using ..Ahorn, Maple

@mapdef Entity "DesoloZatnas/WaterThermos" WaterThermos(x::Integer, y::Integer, isWarm::Bool=true, pourRate::Integer=5, puzzleID::String="puzzle_1", refillable::Bool=false, waterAmount::Integer=50)

const placements = Ahorn.PlacementDict(
    "water_thermos" => Ahorn.EntityPlacement(WaterThermos)
)

function Ahorn.selection(entity::WaterThermos)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WaterThermos, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
