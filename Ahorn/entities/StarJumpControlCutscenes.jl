module StarJumpControlCutscenes

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/StarJumpControlCutscenes" StarJumpControlCutscenes(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "StarJumpControlCutscenes" => Ahorn.EntityPlacement(StarJumpControlCutscenes)
)

function Ahorn.selection(entity::StarJumpControlCutscenes)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::StarJumpControlCutscenes, room::Maple.Room)
    Ahorn.drawSprite(ctx, "@Internal@/northern_lights", entity.x, entity.y)
end

end
