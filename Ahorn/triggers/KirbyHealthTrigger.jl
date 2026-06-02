module KirbyHealthTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/KirbyHealthTrigger" KirbyHealthTrigger(x::Integer, y::Integer, enableHealth::Bool=true, fullHeal::Bool=false, healAmount::Integer=0, height::Integer=16, maxHealth::Integer=6, onlyOnce::Bool=true, setRespawnPoint::Bool=false, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(KirbyHealthTrigger)
)

function Ahorn.selection(entity::KirbyHealthTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbyHealthTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=0, max=inf
# Basic node rendering not implemented in auto-generated plugin

end
