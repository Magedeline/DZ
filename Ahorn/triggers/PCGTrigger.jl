module PCGTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/PCGTrigger" PCGTrigger(x::Integer, y::Integer, height::Integer=16, preset::String="default", roomCount::Integer=8, seed::Integer=-1, targetRoom::String="", trainFromMap::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(PCGTrigger)
)

function Ahorn.selection(entity::PCGTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PCGTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
