module CharaChaser2

using ..Ahorn, Maple

@mapdef Entity "DZ/CharaChaser2" CharaChaser2(x::Integer, y::Integer, aggressive::Bool=false, canChangeMusic::Bool=true, speedMultiplier::Number=1.25)

const placements = Ahorn.PlacementDict(
    "DZ/CharaChaser2" => Ahorn.EntityPlacement(CharaChaser2)
)

function Ahorn.selection(entity::CharaChaser2)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CharaChaser2, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/chara/idle00", entity.x, entity.y)
end

end
