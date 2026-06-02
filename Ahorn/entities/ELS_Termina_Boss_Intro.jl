module ELS_Termina_Boss_Intro

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/ELS_Termina_Boss_Intro" ELS_Termina_Boss_Intro(x::Integer, y::Integer, activationMode::String="touch", completionFlag::String="ch21_els_termina_boss_intro", removeAfterTrigger::Bool=true, requireFlag::String="", showSprite::Bool=false, texturePath::String="objects/Ingeste/sampleEntity/idle00")

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(ELS_Termina_Boss_Intro)
)

function Ahorn.selection(entity::ELS_Termina_Boss_Intro)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ELS_Termina_Boss_Intro, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/Ingeste/sampleEntity/idle00", entity.x, entity.y)
end

end
