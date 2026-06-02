module ELSTerminaHealth

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/ELSTerminaHealth" ELSTerminaHealth(x::Integer, y::Integer, hardMode::Bool=false, maxHealth::Integer=300)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(ELSTerminaHealth)
)

function Ahorn.selection(entity::ELSTerminaHealth)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ELSTerminaHealth, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
