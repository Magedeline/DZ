module BossIntroTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/BossIntroTrigger" BossIntroTrigger(x::Integer, y::Integer, bossName::String="", dialogId::String="", height::Integer=16, musicEvent::String="", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "BossIntroTrigger" => Ahorn.EntityPlacement(BossIntroTrigger)
)

function Ahorn.selection(entity::BossIntroTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BossIntroTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
