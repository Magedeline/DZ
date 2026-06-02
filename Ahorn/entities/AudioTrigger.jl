module AudioTrigger

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/AudioTrigger" AudioTrigger(x::Integer, y::Integer, audioCategory::String="sfx", audioItem::String="kirby", audioSubcategory::String="dialogue", customGuid::String="", eventName::String="", height::Integer=16, oneUse::Bool=false, persistent::Bool=false, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "Audio Trigger" => Ahorn.EntityPlacement(AudioTrigger)
)

function Ahorn.selection(entity::AudioTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AudioTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
