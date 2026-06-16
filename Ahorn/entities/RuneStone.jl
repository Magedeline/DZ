module RuneStone

using ..Ahorn, Maple

@mapdef Entity "DZ/RuneStone" RuneStone(x::Integer, y::Integer, runeId::String="rune_1")

const placements = Ahorn.PlacementDict(
    "RuneGate" => Ahorn.EntityPlacement(RuneStone)
)

function Ahorn.selection(entity::RuneStone)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RuneStone, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
