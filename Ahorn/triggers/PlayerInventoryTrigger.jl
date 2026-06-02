module PlayerInventoryTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/PlayerInventoryTrigger" PlayerInventoryTrigger(x::Integer, y::Integer, backpack::Bool=true, dashes::Integer=3, dreamDash::Bool=false, height::Integer=16, inventoryType::String="KirbyPlayer", kirbyPower::String="None", noRefills::Bool=false, playerState::String="NoChange", requiredFlag::String="", triggerOnce::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(PlayerInventoryTrigger),
    "enable_player" => Ahorn.EntityPlacement(PlayerInventoryTrigger),
    "disable_player" => Ahorn.EntityPlacement(PlayerInventoryTrigger),
    "custom_inventory" => Ahorn.EntityPlacement(PlayerInventoryTrigger)
)

function Ahorn.selection(entity::PlayerInventoryTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PlayerInventoryTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
