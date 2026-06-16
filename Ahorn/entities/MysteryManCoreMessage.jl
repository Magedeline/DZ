module MysteryManCoreMessage

using ..Ahorn, Maple

@mapdef Entity "DZ/MysteryManCoreMessage" MysteryManCoreMessage(x::Integer, y::Integer, baseColor::String="E0FFFF", dialogKey::String="CH18_ENDING", line::Integer=0, shimmerColor::String="FFD700", shimmerSpeed::Number=2.0, textScale::Number=1.25, useRainbowShimmer::Bool=false, visibilityDistance::Number=128.0)

const placements = Ahorn.PlacementDict(
    "MysteryManCoreMessage" => Ahorn.EntityPlacement(MysteryManCoreMessage),
    "mystery_man_cyan" => Ahorn.EntityPlacement(MysteryManCoreMessage),
    "mystery_man_golden" => Ahorn.EntityPlacement(MysteryManCoreMessage),
    "mystery_man_rainbow" => Ahorn.EntityPlacement(MysteryManCoreMessage),
    "mystery_man_void" => Ahorn.EntityPlacement(MysteryManCoreMessage),
    "madeline" => Ahorn.EntityPlacement(MysteryManCoreMessage),
    "badeline" => Ahorn.EntityPlacement(MysteryManCoreMessage)
)

function Ahorn.selection(entity::MysteryManCoreMessage)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MysteryManCoreMessage, room::Maple.Room)
    Ahorn.drawSprite(ctx, "collectables/heartGem/0/00", entity.x, entity.y)
end

end
