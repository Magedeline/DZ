module DistortionField

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/DistortionField" DistortionField(x::Integer, y::Integer, distortionType::String="Reverse", height::Integer=32, intensity::Number=1.0, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "reverse" => Ahorn.EntityPlacement(DistortionField),
    "random" => Ahorn.EntityPlacement(DistortionField),
    "gravity_flip" => Ahorn.EntityPlacement(DistortionField),
    "slow_motion" => Ahorn.EntityPlacement(DistortionField)
)

function Ahorn.selection(entity::DistortionField)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DistortionField, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
