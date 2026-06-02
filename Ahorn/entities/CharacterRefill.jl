module CharacterRefill

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/CharacterRefill" CharacterRefill(x::Integer, y::Integer, characterModeOnly::Bool=false, characterType::Integer=0, customSoundEvent::String="", customSpritePath::String="", dashCount::Integer=1, grantsSpecialAbility::Bool=true, oneUse::Bool=false, refillStamina::Bool=true)

const placements = Ahorn.PlacementDict(
    "kirby" => Ahorn.EntityPlacement(CharacterRefill),
    "madeline" => Ahorn.EntityPlacement(CharacterRefill),
    "badeline" => Ahorn.EntityPlacement(CharacterRefill),
    "theo" => Ahorn.EntityPlacement(CharacterRefill),
    "granny" => Ahorn.EntityPlacement(CharacterRefill),
    "oshiro" => Ahorn.EntityPlacement(CharacterRefill),
    "chara" => Ahorn.EntityPlacement(CharacterRefill),
    "frisk" => Ahorn.EntityPlacement(CharacterRefill),
    "ralsei" => Ahorn.EntityPlacement(CharacterRefill),
    "asriel" => Ahorn.EntityPlacement(CharacterRefill),
    "meta_knight" => Ahorn.EntityPlacement(CharacterRefill),
    "king_dedede" => Ahorn.EntityPlacement(CharacterRefill),
    "magolor" => Ahorn.EntityPlacement(CharacterRefill),
    "mage_kirby" => Ahorn.EntityPlacement(CharacterRefill)
)

function Ahorn.selection(entity::CharacterRefill)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CharacterRefill, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
