module ColorGradeEffect

using ..Ahorn, Maple

@mapdef Effect "MaggyHelper/ColorGradeEffect" ColorGradeEffect(x::Integer, y::Integer, intensity::Number=1.0, lutTextureName::String="default")

const placements = Ahorn.PlacementDict(
    "colorgrade" => Ahorn.EntityPlacement(ColorGradeEffect)
)

function Ahorn.selection(entity::ColorGradeEffect)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ColorGradeEffect, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
