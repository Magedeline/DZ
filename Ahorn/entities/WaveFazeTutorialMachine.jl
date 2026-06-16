module WaveFazeTutorialMachine

using ..Ahorn, Maple

@mapdef Entity "DZ/WaveFazeTutorialMachine" WaveFazeTutorialMachine(x::Integer, y::Integer, difficulty::String="easy", isActive::Bool=true, machineType::String="tutorial")

const placements = Ahorn.PlacementDict(
    "WaveFazeTutorialMachine" => Ahorn.EntityPlacement(WaveFazeTutorialMachine),
    "WaveFazeTutorialMachine_advanced" => Ahorn.EntityPlacement(WaveFazeTutorialMachine)
)

function Ahorn.selection(entity::WaveFazeTutorialMachine)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WaveFazeTutorialMachine, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/kevins_pc/pc_idle", entity.x, entity.y)
end

end
