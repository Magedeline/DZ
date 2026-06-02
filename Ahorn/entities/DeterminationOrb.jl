module DeterminationOrb

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/DeterminationOrb" DeterminationOrb(x::Integer, y::Integer, dashBoost::Integer=1, duration::Number=10.0, oneUse::Bool=true, speedMultiplier::Number=1.2)

const placements = Ahorn.PlacementDict(
    "Determination Orb" => Ahorn.EntityPlacement(DeterminationOrb),
    "Determination Orb (Strong)" => Ahorn.EntityPlacement(DeterminationOrb),
    "Determination Orb (Reusable)" => Ahorn.EntityPlacement(DeterminationOrb)
)

function Ahorn.selection(entity::DeterminationOrb)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DeterminationOrb, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
