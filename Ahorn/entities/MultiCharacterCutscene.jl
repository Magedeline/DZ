module MultiCharacterCutscene

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/MultiCharacterCutscene" MultiCharacterCutscene(x::Integer, y::Integer, autoStart::Bool=true, cutsceneId::String="CH0_MODINTRO")

const placements = Ahorn.PlacementDict(
    "Mod Intro Cutscene" => Ahorn.EntityPlacement(MultiCharacterCutscene),
    "Chapter End Cutscene" => Ahorn.EntityPlacement(MultiCharacterCutscene),
    "Madeline Rest Cutscene" => Ahorn.EntityPlacement(MultiCharacterCutscene),
    "Nightmare Intro Cutscene" => Ahorn.EntityPlacement(MultiCharacterCutscene),
    "Wake Up Cutscene" => Ahorn.EntityPlacement(MultiCharacterCutscene),
    "Memorial Darkside" => Ahorn.EntityPlacement(MultiCharacterCutscene),
    "Poem Cutscene" => Ahorn.EntityPlacement(MultiCharacterCutscene)
)

function Ahorn.selection(entity::MultiCharacterCutscene)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MultiCharacterCutscene, room::Maple.Room)
    Ahorn.drawSprite(ctx, "scenery/memorial/memorial", entity.x, entity.y)
end

# Nodes: min=0, max=inf
# Basic node rendering not implemented in auto-generated plugin

end
