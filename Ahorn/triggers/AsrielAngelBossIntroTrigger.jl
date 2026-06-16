module AsrielAngelBossIntroTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/AsrielAngelBossIntroTrigger" AsrielAngelBossIntroTrigger(x::Integer, y::Integer, dialogKey::String="ch20_asriel_angel_boss_intro", height::Integer=16, requireFlag::String="", requireNotFlag::String="asriel_angel_boss_intro", shakeIntensity::Number=1.0, triggerOnce::Bool=true, width::Integer=32, zoomDuration::Number=0.6)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(AsrielAngelBossIntroTrigger),
    "room_specific" => Ahorn.EntityPlacement(AsrielAngelBossIntroTrigger),
    "no_zoom" => Ahorn.EntityPlacement(AsrielAngelBossIntroTrigger)
)

function Ahorn.selection(entity::AsrielAngelBossIntroTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AsrielAngelBossIntroTrigger, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entityTrigger", entity.x, entity.y)
end

# Nodes: min=0, max=inf
# Basic node rendering not implemented in auto-generated plugin

end
