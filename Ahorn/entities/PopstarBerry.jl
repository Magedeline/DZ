module PopstarBerry

using ..Ahorn, Maple

@mapdef Entity "DZ/PopstarBerry" PopstarBerry(x::Integer, y::Integer, collectSound::String="Elaborate", customCollectSound::String="", levelSet::String="Maggy/DESOLO_ZANTAS/19-Space", maps::String="", requires::String="")

const placements = Ahorn.PlacementDict(
    "PopstarBerry" => Ahorn.EntityPlacement(PopstarBerry)
)

function Ahorn.selection(entity::PopstarBerry)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PopstarBerry, room::Maple.Room)
    Ahorn.drawSprite(ctx, "collectables/maggy/popstarberry/Maggy/DesoloZantas/spin/000", entity.x, entity.y)
end

end
