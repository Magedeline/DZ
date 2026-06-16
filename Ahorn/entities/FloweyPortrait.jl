module FloweyPortrait

using ..Ahorn, Maple

@mapdef Entity "DZ/FloweyPortrait" FloweyPortrait(x::Integer, y::Integer, autoTrigger::Bool=false, dialogTrigger::String="{portrait flowey glitchyfreak}", displayDuration::Number=5.0, enableDistortion::Bool=true, enableParticles::Bool=true, glitchIntensity::Number=5.0, portraitType::String="glitchyfreak", soundEffect::String="", triggerOnDialog::Bool=true)

const placements = Ahorn.PlacementDict(
    "glitchy_freak" => Ahorn.EntityPlacement(FloweyPortrait),
    "glitchy_creepy" => Ahorn.EntityPlacement(FloweyPortrait),
    "glitchy_panic" => Ahorn.EntityPlacement(FloweyPortrait),
    "glitchy_revenge" => Ahorn.EntityPlacement(FloweyPortrait),
    "glitchy_angry" => Ahorn.EntityPlacement(FloweyPortrait),
    "glitchy_smirk" => Ahorn.EntityPlacement(FloweyPortrait),
    "glitchy_generic" => Ahorn.EntityPlacement(FloweyPortrait)
)

function Ahorn.selection(entity::FloweyPortrait)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FloweyPortrait, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
