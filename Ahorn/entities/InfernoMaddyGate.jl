module InfernoMaddyGate

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/InfernoMaddyGate" InfernoMaddyGate(x::Integer, y::Integer, height::Integer=48, sprite::String="default", type::String="CloseBehindKglobal::Player")

const placements = Ahorn.PlacementDict(
    "close_behind_Kglobal::Player" => Ahorn.EntityPlacement(InfernoMaddyGate),
    "close_behind_Kglobal::Player_always" => Ahorn.EntityPlacement(InfernoMaddyGate),
    "nearest_switch" => Ahorn.EntityPlacement(InfernoMaddyGate),
    "holding_theo" => Ahorn.EntityPlacement(InfernoMaddyGate),
    "touch_switches" => Ahorn.EntityPlacement(InfernoMaddyGate)
)

function Ahorn.selection(entity::InfernoMaddyGate)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::InfernoMaddyGate, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/Maggy/DesoloZantas/door/templeDoorB00", entity.x, entity.y)
end

end
