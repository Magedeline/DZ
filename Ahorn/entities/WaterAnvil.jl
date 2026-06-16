module WaterAnvil

using ..Ahorn, Maple

@mapdef Entity "DesoloZatnas/WaterAnvil" WaterAnvil(x::Integer, y::Integer, affectsBalance::Bool=true, canPickUp::Bool=true, puzzleID::String="puzzle_1", requiresStrength::Bool=false, weight::Integer=20)

const placements = Ahorn.PlacementDict(
    "water_anvil" => Ahorn.EntityPlacement(WaterAnvil)
)

function Ahorn.selection(entity::WaterAnvil)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WaterAnvil, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
