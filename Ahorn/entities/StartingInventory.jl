module StartingInventory

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/StartingInventory" StartingInventory(x::Integer, y::Integer, debugVisible::Bool=false, inventoryType::String="Default")

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(StartingInventory),
    "heart_power" => Ahorn.EntityPlacement(StartingInventory),
    "kirby_Kglobal::Player" => Ahorn.EntityPlacement(StartingInventory),
    "say_goodbye" => Ahorn.EntityPlacement(StartingInventory),
    "titan_tower_climbing" => Ahorn.EntityPlacement(StartingInventory),
    "corruption" => Ahorn.EntityPlacement(StartingInventory),
    "the_end" => Ahorn.EntityPlacement(StartingInventory)
)

function Ahorn.selection(entity::StartingInventory)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::StartingInventory, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
