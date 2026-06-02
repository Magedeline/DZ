module PlateauMod

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/PlateauMod" PlateauMod(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "PlateauMod" => Ahorn.EntityPlacement(PlateauMod)
)

function Ahorn.selection(entity::PlateauMod)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PlateauMod, room::Maple.Room)
    Ahorn.drawSprite(ctx, "scenery/fallplateau", entity.x, entity.y)
end

end
