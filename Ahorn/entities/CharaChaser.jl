module CharaChaser

using ..Ahorn, Maple

@mapdef Entity "DZ/CharaChaser" CharaChaser(x::Integer, y::Integer, aggressive::Bool=false, canChangeMusic::Bool=true, speedMultiplier::Number=1.0)

const placements = Ahorn.PlacementDict(
    "DZ/CharaChaser" => Ahorn.EntityPlacement(CharaChaser),
    "CharaChaser (Aggressive)" => Ahorn.EntityPlacement(CharaChaser)
)

function Ahorn.selection(entity::CharaChaser)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CharaChaser, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/chara/idle00", entity.x, entity.y)
end

end
