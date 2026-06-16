module BossFightTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/BossFightTrigger" BossFightTrigger(x::Integer, y::Integer, bossMusic::String="event:/music/lvl9/main", bossType::String="KirbyBoss", height::Integer=16, lockRoom::Bool=true, playMusic::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "BossFightTrigger" => Ahorn.EntityPlacement(BossFightTrigger)
)

function Ahorn.selection(entity::BossFightTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BossFightTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
