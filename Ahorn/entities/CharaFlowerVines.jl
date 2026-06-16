module CharaFlowerVines

using ..Ahorn, Maple

@mapdef Entity "DZ/CharaFlowerVines" CharaFlowerVines(x::Integer, y::Integer, fear_distance::String="", slide_until::Integer=0)

const placements = Ahorn.PlacementDict(
    "Chara Flower Vines" => Ahorn.EntityPlacement(CharaFlowerVines)
)

function Ahorn.selection(entity::CharaFlowerVines)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CharaFlowerVines, room::Maple.Room)
    Ahorn.drawSprite(ctx, "@Internal@/tentacles", entity.x, entity.y)
end

end
