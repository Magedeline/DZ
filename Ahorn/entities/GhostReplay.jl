module GhostReplay

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/GhostReplay" GhostReplay(x::Integer, y::Integer, recordOnFlag::String="record_ghost", replayOnFlag::String="replay_ghost")

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(GhostReplay)
)

function Ahorn.selection(entity::GhostReplay)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GhostReplay, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/Kglobal::Player/idle00", entity.x, entity.y)
end

end
