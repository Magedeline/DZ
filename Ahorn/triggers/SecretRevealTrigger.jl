module SecretRevealTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/SecretRevealTrigger" SecretRevealTrigger(x::Integer, y::Integer, flag::String="secret_found", height::Integer=16, revealSound::String="", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "SecretRevealTrigger" => Ahorn.EntityPlacement(SecretRevealTrigger)
)

function Ahorn.selection(entity::SecretRevealTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SecretRevealTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
