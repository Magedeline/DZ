module WeightSwitch

using ..Ahorn, Maple

@mapdef Entity "DZ/WeightSwitch" WeightSwitch(x::Integer, y::Integer, flag::String="weight_switch", requiredWeight::Number=1.0, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "WeightSwitch" => Ahorn.EntityPlacement(WeightSwitch)
)

function Ahorn.selection(entity::WeightSwitch)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WeightSwitch, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
