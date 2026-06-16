module CheckpointTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/CheckpointTrigger" CheckpointTrigger(x::Integer, y::Integer, checkpointId::String="cp_1", height::Integer=16, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "CheckpointTrigger" => Ahorn.EntityPlacement(CheckpointTrigger)
)

function Ahorn.selection(entity::CheckpointTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CheckpointTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
