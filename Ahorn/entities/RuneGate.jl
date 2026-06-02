module RuneGate

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/RuneGate" RuneGate(x::Integer, y::Integer, gateId::String="gate_1", height::Integer=24, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "RuneGate" => Ahorn.EntityPlacement(RuneGate)
)

function Ahorn.selection(entity::RuneGate)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RuneGate, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
