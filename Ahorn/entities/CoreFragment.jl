module CoreFragment

using ..Ahorn, Maple

@mapdef Entity "DZ/CoreFragment" CoreFragment(x::Integer, y::Integer, fragmentId::String="", fragmentIndex::Integer=1, requiredShields::Integer=3, requiresProtection::Bool=true)

const placements = Ahorn.PlacementDict(
    "protected" => Ahorn.EntityPlacement(CoreFragment),
    "open" => Ahorn.EntityPlacement(CoreFragment)
)

function Ahorn.selection(entity::CoreFragment)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CoreFragment, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
