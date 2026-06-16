module NightmareFragment

using ..Ahorn, Maple

@mapdef Entity "DZ/NightmareFragment" NightmareFragment(x::Integer, y::Integer, fragmentId::String="", fragmentNumber::Integer=1)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(NightmareFragment)
)

function Ahorn.selection(entity::NightmareFragment)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NightmareFragment, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
