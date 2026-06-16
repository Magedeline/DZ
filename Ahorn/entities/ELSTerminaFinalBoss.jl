module ELSTerminaFinalBoss

using ..Ahorn, Maple

@mapdef Entity "DZ/ELSTerminaFinalBoss" ELSTerminaFinalBoss(x::Integer, y::Integer, difficultyMode::Integer=0, fromCutscene::Bool=false, hasFiveHeartGems::Bool=false)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(ELSTerminaFinalBoss)
)

function Ahorn.selection(entity::ELSTerminaFinalBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ELSTerminaFinalBoss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
