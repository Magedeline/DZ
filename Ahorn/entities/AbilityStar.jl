module AbilityStar

using ..Ahorn, Maple

@mapdef Entity "DZ/AbilityStar" AbilityStar(x::Integer, y::Integer, ability::String="Fire")

const placements = Ahorn.PlacementDict(
    "fire" => Ahorn.EntityPlacement(AbilityStar),
    "ice" => Ahorn.EntityPlacement(AbilityStar),
    "sword" => Ahorn.EntityPlacement(AbilityStar),
    "beam" => Ahorn.EntityPlacement(AbilityStar),
    "spark" => Ahorn.EntityPlacement(AbilityStar),
    "stone" => Ahorn.EntityPlacement(AbilityStar),
    "bomb" => Ahorn.EntityPlacement(AbilityStar),
    "hammer" => Ahorn.EntityPlacement(AbilityStar),
    "ninja" => Ahorn.EntityPlacement(AbilityStar),
    "cutter" => Ahorn.EntityPlacement(AbilityStar)
)

function Ahorn.selection(entity::AbilityStar)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AbilityStar, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
