module RandomizerTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/RandomizerTrigger" RandomizerTrigger(x::Integer, y::Integer, height::Integer=16, randomizeAbilities::Bool=true, randomizeGravity::Bool=false, randomizeSpeed::Bool=false, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "RandomizerTrigger" => Ahorn.EntityPlacement(RandomizerTrigger)
)

function Ahorn.selection(entity::RandomizerTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RandomizerTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
