module VoidTendril

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/VoidTendril" VoidTendril(x::Integer, y::Integer, damage::Integer=1, phaseWindow::Number=0.3, phaseableOnDash::Bool=true, swaySpeed::Number=1.0)

const placements = Ahorn.PlacementDict(
    "Void Tendril" => Ahorn.EntityPlacement(VoidTendril),
    "Void Tendril (Impassable)" => Ahorn.EntityPlacement(VoidTendril)
)

function Ahorn.selection(entity::VoidTendril)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::VoidTendril, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
