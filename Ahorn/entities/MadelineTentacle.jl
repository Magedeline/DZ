module MadelineTentacle

using ..Ahorn, Maple

@mapdef Entity "DZ/MadelineTentacle" MadelineTentacle(x::Integer, y::Integer, animationSpeed::Number=1.0, attackTarget::String="flowey", autoGrow::Bool=false, canBeDeflected::Bool=true, color::String="darkred", dialogTrigger::String="dz_trigger 0 madeline tentacle appear", enableParticles::Bool=true, growthStage::Integer=1, maxLength::Number=100.0, persistent::Bool=false, tentacleType::String="appear", thickness::Number=8.0, triggerOnDialog::Bool=true)

const placements = Ahorn.PlacementDict(
    "appear" => Ahorn.EntityPlacement(MadelineTentacle),
    "appear_more" => Ahorn.EntityPlacement(MadelineTentacle),
    "grow_more" => Ahorn.EntityPlacement(MadelineTentacle),
    "attack" => Ahorn.EntityPlacement(MadelineTentacle)
)

function Ahorn.selection(entity::MadelineTentacle)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MadelineTentacle, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
