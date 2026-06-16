module CeremonyFlame

using ..Ahorn, Maple

@mapdef Entity "DZ/CeremonyFlame" CeremonyFlame(x::Integer, y::Integer, canSpread::Bool=true, isSource::Bool=true, maxSpreadDistance::Integer=200, spreadSpeed::Integer=20)

const placements = Ahorn.PlacementDict(
    "source" => Ahorn.EntityPlacement(CeremonyFlame),
    "static" => Ahorn.EntityPlacement(CeremonyFlame)
)

function Ahorn.selection(entity::CeremonyFlame)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CeremonyFlame, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
