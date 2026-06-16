module PayphoneCutscene

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/PayphoneCutscene" PayphoneCutscene(x::Integer, y::Integer, cutsceneType::String="dream")

const placements = Ahorn.PlacementDict(
    "Payphone Dream Cutscene" => Ahorn.EntityPlacement(PayphoneCutscene),
    "Payphone Awake Cutscene" => Ahorn.EntityPlacement(PayphoneCutscene)
)

function Ahorn.selection(entity::PayphoneCutscene)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PayphoneCutscene, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/payphone/payphone00", entity.x, entity.y)
end

# Nodes: min=0, max=inf
# Basic node rendering not implemented in auto-generated plugin

end
