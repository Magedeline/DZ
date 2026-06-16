module VesselCreationTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/VesselCreationTrigger" VesselCreationTrigger(x::Integer, y::Integer, autoStart::Bool=false, height::Integer=16, triggerOnce::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "VesselCreationTrigger" => Ahorn.EntityPlacement(VesselCreationTrigger),
    "auto_start" => Ahorn.EntityPlacement(VesselCreationTrigger)
)

function Ahorn.selection(entity::VesselCreationTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::VesselCreationTrigger, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/vessel_creation_trigger", entity.x, entity.y)
end

end
