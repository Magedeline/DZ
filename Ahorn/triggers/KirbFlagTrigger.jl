module KirbFlagTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/KirbFlagTrigger" KirbFlagTrigger(x::Integer, y::Integer, height::Integer=16, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "kirb_flag_trigger" => Ahorn.EntityPlacement(KirbFlagTrigger)
)

function Ahorn.selection(entity::KirbFlagTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbFlagTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
