module MaddyCrystalPedestal

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/MaddyCrystalPedestal" MaddyCrystalPedestal(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "MaddyCrystalPedestal" => Ahorn.EntityPlacement(MaddyCrystalPedestal)
)

function Ahorn.selection(entity::MaddyCrystalPedestal)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MaddyCrystalPedestal, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
