module KirbyTutorialBird

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/KirbyTutorialBird" KirbyTutorialBird(x::Integer, y::Integer, birdId::String="", caw::Bool=true, controls::String="", dialogs::String="", faceLeft::Bool=true, onlyOnce::Bool=false, startupIndex::Integer=0, triggerOnce::Bool=true)

const placements = Ahorn.PlacementDict(
    "tutorial_bird" => Ahorn.EntityPlacement(KirbyTutorialBird),
    "aqua_hook_intro" => Ahorn.EntityPlacement(KirbyTutorialBird),
    "aqua_hook_swing" => Ahorn.EntityPlacement(KirbyTutorialBird),
    "method_btn_dash_jump" => Ahorn.EntityPlacement(KirbyTutorialBird),
    "method_compound_tokens" => Ahorn.EntityPlacement(KirbyTutorialBird),
    "method_mod_binding" => Ahorn.EntityPlacement(KirbyTutorialBird),
    "controls_method_reference" => Ahorn.EntityPlacement(KirbyTutorialBird)
)

function Ahorn.selection(entity::KirbyTutorialBird)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbyTutorialBird, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/bird/crow00", entity.x, entity.y)
end

end
