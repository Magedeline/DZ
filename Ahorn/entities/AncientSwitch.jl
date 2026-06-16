module AncientSwitch

using ..Ahorn, Maple

@mapdef Entity "DZ/AncientSwitch" AncientSwitch(x::Integer, y::Integer, isActivated::Bool=false, persistent::Bool=true, requiresWeight::Bool=false, switchType::String="pressure", targetEntity::String="")

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(AncientSwitch),
    "lever" => Ahorn.EntityPlacement(AncientSwitch),
    "crystal" => Ahorn.EntityPlacement(AncientSwitch)
)

function Ahorn.selection(entity::AncientSwitch)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AncientSwitch, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/Maggy/DesoloZantas/temple/switch00", entity.x, entity.y)
end

end
