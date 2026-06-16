module WaterLever

using ..Ahorn, Maple

@mapdef Entity "DesoloZatnas/WaterLever" WaterLever(x::Integer, y::Integer, flowAmount::Integer=10, flowDirection::String="increase", leverType::String="standard", oneUse::Bool=false, puzzleID::String="puzzle_1", requiresHold::Bool=false)

const placements = Ahorn.PlacementDict(
    "water_lever" => Ahorn.EntityPlacement(WaterLever)
)

function Ahorn.selection(entity::WaterLever)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WaterLever, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
