module heart_power

using ..Ahorn, Maple

@mapdef Entity "heart_power" heart_power(x::Integer, y::Integer, height::Integer=16, inventoryType::String="Heart", oneUse::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "heart_power" => Ahorn.EntityPlacement(heart_power),
    "kirby_Kglobal::Player" => Ahorn.EntityPlacement(heart_power),
    "say_goodbye" => Ahorn.EntityPlacement(heart_power),
    "titan_tower_climbing" => Ahorn.EntityPlacement(heart_power),
    "corruption" => Ahorn.EntityPlacement(heart_power),
    "the_end" => Ahorn.EntityPlacement(heart_power),
    "reset_default" => Ahorn.EntityPlacement(heart_power)
)

function Ahorn.selection(entity::heart_power)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::heart_power, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
