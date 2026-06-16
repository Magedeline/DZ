module BlackholeFlareBlocker

using ..Ahorn, Maple

@mapdef Trigger "DZ/BlackholeFlareBlocker" BlackholeFlareBlocker(x::Integer, y::Integer, affectsRiser::Bool=true, affectsSideway::Bool=true, behavior::String="Stop", height::Integer=64, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "stop" => Ahorn.EntityPlacement(BlackholeFlareBlocker),
    "reverse" => Ahorn.EntityPlacement(BlackholeFlareBlocker),
    "destroy" => Ahorn.EntityPlacement(BlackholeFlareBlocker),
    "sideway_only" => Ahorn.EntityPlacement(BlackholeFlareBlocker),
    "riser_only" => Ahorn.EntityPlacement(BlackholeFlareBlocker)
)

function Ahorn.selection(entity::BlackholeFlareBlocker)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BlackholeFlareBlocker, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
