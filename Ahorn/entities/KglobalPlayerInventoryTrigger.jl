module KglobalPlayerInventoryTrigger

using ..Ahorn, Maple

@mapdef Entity "DZ/Kglobal::PlayerInventoryTrigger" KglobalPlayerInventoryTrigger(x::Integer, y::Integer, height::Integer=16, inventoryType::String="Heart", oneUse::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "heart_power" => Ahorn.EntityPlacement(KglobalPlayerInventoryTrigger),
    "kirby_Kglobal::Player" => Ahorn.EntityPlacement(KglobalPlayerInventoryTrigger),
    "say_goodbye" => Ahorn.EntityPlacement(KglobalPlayerInventoryTrigger),
    "titan_tower_climbing" => Ahorn.EntityPlacement(KglobalPlayerInventoryTrigger),
    "corruption" => Ahorn.EntityPlacement(KglobalPlayerInventoryTrigger),
    "the_end" => Ahorn.EntityPlacement(KglobalPlayerInventoryTrigger),
    "reset_default" => Ahorn.EntityPlacement(KglobalPlayerInventoryTrigger)
)

function Ahorn.selection(entity::KglobalPlayerInventoryTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KglobalPlayerInventoryTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
