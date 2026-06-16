module CharacterSwapTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/CharacterSwapTrigger" CharacterSwapTrigger(x::Integer, y::Integer, height::Integer=16, targetCharacter::String="Kirby", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "CharacterSwapTrigger" => Ahorn.EntityPlacement(CharacterSwapTrigger)
)

function Ahorn.selection(entity::CharacterSwapTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CharacterSwapTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
