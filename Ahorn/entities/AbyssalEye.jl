module AbyssalEye

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/AbyssalEye" AbyssalEye(x::Integer, y::Integer, gazeRange::Integer=200, gazeWidth::Integer=30, health::Integer=2)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(AbyssalEye)
)

function Ahorn.selection(entity::AbyssalEye)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AbyssalEye, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
