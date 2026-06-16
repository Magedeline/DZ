module RewindCrystal

using ..Ahorn, Maple

@mapdef Entity "DZ/RewindCrystal" RewindCrystal(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "RewindCrystal" => Ahorn.EntityPlacement(RewindCrystal),
    "RewindCrystallong" => Ahorn.EntityPlacement(RewindCrystal)
)

function Ahorn.selection(entity::RewindCrystal)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RewindCrystal, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
