module CharaChaserEnd

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/CharaChaserEnd" CharaChaserEnd(x::Integer, y::Integer, height::Integer=16, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "CharaChaserEnd" => Ahorn.EntityPlacement(CharaChaserEnd),
    "wide" => Ahorn.EntityPlacement(CharaChaserEnd),
    "tall" => Ahorn.EntityPlacement(CharaChaserEnd)
)

function Ahorn.selection(entity::CharaChaserEnd)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CharaChaserEnd, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.8, 0.2, 0.2, 0.3), (0.8, 0.2, 0.2, 0.8))
end

end
