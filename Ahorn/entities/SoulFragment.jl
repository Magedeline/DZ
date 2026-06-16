module SoulFragment

using ..Ahorn, Maple

@mapdef Entity "DZ/SoulFragment" SoulFragment(x::Integer, y::Integer, fragmentId::String="", requiredSouls::Integer=3)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(SoulFragment)
)

function Ahorn.selection(entity::SoulFragment)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SoulFragment, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
