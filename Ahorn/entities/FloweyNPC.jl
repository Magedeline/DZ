module FloweyNPC

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/FloweyNPC" FloweyNPC(x::Integer, y::Integer, autoEmerge::Bool=false, dialogId::String="CH10_FLOWEY_INTRO", emergeDelay::Number=0.5, startHidden::Bool=true)

const placements = Ahorn.PlacementDict(
    "Flowey (Hidden - Cutscene)" => Ahorn.EntityPlacement(FloweyNPC),
    "Flowey (Visible)" => Ahorn.EntityPlacement(FloweyNPC),
    "Flowey (Auto Emerge)" => Ahorn.EntityPlacement(FloweyNPC)
)

function Ahorn.selection(entity::FloweyNPC)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FloweyNPC, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
