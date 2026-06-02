module AxisTerminator2Boss

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/AxisTerminator2Boss" AxisTerminator2Boss(x::Integer, y::Integer, health::Integer=800, maxHealth::Integer=800, phase2Active::Bool=false)

const placements = Ahorn.PlacementDict(
    "axis_terminator_2_boss" => Ahorn.EntityPlacement(AxisTerminator2Boss)
)

function Ahorn.selection(entity::AxisTerminator2Boss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AxisTerminator2Boss, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/axis2/axis2_idle00", entity.x, entity.y)
end

end
