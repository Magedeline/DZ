module BossArenaTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/BossArenaTrigger" BossArenaTrigger(x::Integer, y::Integer, bossEntityType::String="", bossName::String="Boss", createHealthUI::Bool=true, height::Integer=16, showHealthBar::Bool=true, startEncounter::Bool=false, triggerOnce::Bool=false, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "Boss Arena Trigger" => Ahorn.EntityPlacement(BossArenaTrigger),
    "Boss Actor Encounter" => Ahorn.EntityPlacement(BossArenaTrigger)
)

function Ahorn.selection(entity::BossArenaTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BossArenaTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
