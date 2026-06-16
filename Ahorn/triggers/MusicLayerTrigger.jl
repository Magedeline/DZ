module MusicLayerTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/MusicLayerTrigger" MusicLayerTrigger(x::Integer, y::Integer, enabled::Bool=true, height::Integer=16, layerIndex::Integer=1, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "MusicLayerTrigger" => Ahorn.EntityPlacement(MusicLayerTrigger)
)

function Ahorn.selection(entity::MusicLayerTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MusicLayerTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
