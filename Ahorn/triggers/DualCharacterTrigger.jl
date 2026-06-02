module DualCharacterTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/DualCharacterTrigger" DualCharacterTrigger(x::Integer, y::Integer, height::Integer=16, secondCharacter::String="Kirby", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "DualCharacterTrigger" => Ahorn.EntityPlacement(DualCharacterTrigger)
)

function Ahorn.selection(entity::DualCharacterTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DualCharacterTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
