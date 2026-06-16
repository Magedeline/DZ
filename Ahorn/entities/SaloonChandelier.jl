module SaloonChandelier

using ..Ahorn, Maple

@mapdef Entity "DZ/SaloonChandelier" SaloonChandelier(x::Integer, y::Integer, canFall::Bool=true, chainLength::Integer=80, isHazard::Bool=true, swingAngle::Number=0.4, swingPeriod::Number=3.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(SaloonChandelier)
)

function Ahorn.selection(entity::SaloonChandelier)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SaloonChandelier, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
