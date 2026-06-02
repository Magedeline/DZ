module BeyondSummitGemManager

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/BeyondSummitGemManager" BeyondSummitGemManager(x::Integer, y::Integer, flag::String="beyondsummit_gate_open")

const placements = Ahorn.PlacementDict(
    "BeyondSummitGemManager" => Ahorn.EntityPlacement(BeyondSummitGemManager)
)

function Ahorn.selection(entity::BeyondSummitGemManager)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BeyondSummitGemManager, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=0, max=7
# Basic node rendering not implemented in auto-generated plugin

end
