module WindTunnel

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/WindTunnel" WindTunnel(x::Integer, y::Integer, direction::String="Up", height::Integer=64, strength::Number=200.0, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "up" => Ahorn.EntityPlacement(WindTunnel),
    "down" => Ahorn.EntityPlacement(WindTunnel),
    "left" => Ahorn.EntityPlacement(WindTunnel),
    "right" => Ahorn.EntityPlacement(WindTunnel)
)

function Ahorn.selection(entity::WindTunnel)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WindTunnel, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.8, 0.9, 1.0, 0.15), (0.8, 0.9, 1.0, 0.4))
end

end
