module KirbyHealthRoomController

using ..Ahorn, Maple

@mapdef Entity "DZ/KirbyHealthRoomController" KirbyHealthRoomController(x::Integer, y::Integer, autoHealOnEnter::Bool=false, maxHealth::Integer=6, setAsRespawnRoom::Bool=false)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(KirbyHealthRoomController)
)

function Ahorn.selection(entity::KirbyHealthRoomController)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbyHealthRoomController, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/resortclutter/book_stack_c", entity.x, entity.y)
end

end
