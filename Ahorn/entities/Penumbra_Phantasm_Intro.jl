module Penumbra_Phantasm_Intro

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/Penumbra_Phantasm_Intro" Penumbra_Phantasm_Intro(x::Integer, y::Integer, activationMode::String="touch", completionFlag::String="ch20_penumbra_phantasm_intro", removeAfterTrigger::Bool=true, requireFlag::String="", showSprite::Bool=false, texturePath::String="objects/Ingeste/sampleEntity/idle00")

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(Penumbra_Phantasm_Intro)
)

function Ahorn.selection(entity::Penumbra_Phantasm_Intro)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Penumbra_Phantasm_Intro, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/Ingeste/sampleEntity/idle00", entity.x, entity.y)
end

end
