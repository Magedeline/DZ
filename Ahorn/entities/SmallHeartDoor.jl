module SmallHeartDoor

using ..Ahorn, Maple

@mapdef Entity "DZ/SmallHeartDoor" SmallHeartDoor(
    x::Integer, y::Integer,
    width::Integer=40, height::Integer=40,
    chapter::Integer=10, requires::Integer=3,
    notEnoughDialog::String="", unlockCutscene::String=""
)

const placements = Ahorn.PlacementDict(
    "small_heart_door" => Ahorn.EntityPlacement(SmallHeartDoor)
)

function Ahorn.selection(entity::SmallHeartDoor)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SmallHeartDoor, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.8, 0.2, 0.4, 0.25), (0.9, 0.3, 0.5, 0.8))
end

end
