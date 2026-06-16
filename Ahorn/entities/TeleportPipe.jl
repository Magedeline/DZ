module TeleportPipe

using ..Ahorn, Maple

@mapdef Entity "DZ/TeleportPipe" TeleportPipe(x::Integer, y::Integer, autoEnter::Bool=false, direction::String="Down", enterDelay::Number=0.5, pipeColor::String="228B22", pipeId::String="pipe_1", pipeType::String="Both", targetPipeId::String="pipe_2", targetRoom::String="", targetX::Integer=0, targetY::Integer=0)

const placements = Ahorn.PlacementDict(
    "DZ/TeleportPipe" => Ahorn.EntityPlacement(TeleportPipe),
    "Teleport Pipe (Up)" => Ahorn.EntityPlacement(TeleportPipe),
    "Teleport Pipe (Left)" => Ahorn.EntityPlacement(TeleportPipe),
    "Teleport Pipe (Right)" => Ahorn.EntityPlacement(TeleportPipe)
)

function Ahorn.selection(entity::TeleportPipe)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TeleportPipe, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/Ingeste/teleportPipe/down_idle00", entity.x, entity.y)
end

end
