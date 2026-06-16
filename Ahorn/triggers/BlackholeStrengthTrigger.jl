module BlackholeStrengthTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/BlackholeStrengthTrigger" BlackholeStrengthTrigger(x::Integer, y::Integer, strength::String="Medium")

const placements = Ahorn.PlacementDict(
    "BlackholeStrengthTrigger" => Ahorn.EntityPlacement(BlackholeStrengthTrigger)
)

function Ahorn.selection(entity::BlackholeStrengthTrigger)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BlackholeStrengthTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
