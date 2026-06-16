module VesselCreationCutscene

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/VesselCreationCutscene" VesselCreationCutscene(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Vessel Creation Cutscene" => Ahorn.EntityPlacement(VesselCreationCutscene)
)

function Ahorn.selection(entity::VesselCreationCutscene)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::VesselCreationCutscene, room::Maple.Room)
    Ahorn.drawSprite(ctx, "decals/7-summit/summit_memorial", entity.x, entity.y)
end

# Nodes: min=0, max=inf
# Basic node rendering not implemented in auto-generated plugin

end
