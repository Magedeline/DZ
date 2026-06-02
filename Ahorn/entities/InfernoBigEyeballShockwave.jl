module InfernoBigEyeballShockwave

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/InfernoBigEyeballShockwave" InfernoBigEyeballShockwave(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(InfernoBigEyeballShockwave)
)

function Ahorn.selection(entity::InfernoBigEyeballShockwave)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::InfernoBigEyeballShockwave, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
