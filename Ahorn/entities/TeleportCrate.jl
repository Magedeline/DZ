module TeleportCrate

using ..Ahorn, Maple

@mapdef Entity "DZ/TeleportCrate" TeleportCrate(x::Integer, y::Integer, teleportRange::Number=120.0, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(TeleportCrate)
)

function Ahorn.selection(entity::TeleportCrate)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TeleportCrate, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
