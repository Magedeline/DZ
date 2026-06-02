module ELSTerminaBoss

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/ELSTerminaBoss" ELSTerminaBoss(x::Integer, y::Integer, fromCutscene::Bool=false, hardMode::Bool=false, phase::Integer=4)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(ELSTerminaBoss)
)

function Ahorn.selection(entity::ELSTerminaBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ELSTerminaBoss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
