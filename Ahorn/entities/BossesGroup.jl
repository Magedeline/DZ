module BossesGroup

using ..Ahorn, Maple

@mapdef Entity "DZ/BossesGroup" BossesGroup(x::Integer, y::Integer, groupName::String="BossGroup", bossNames::String="kingtitan")

const placements = Ahorn.PlacementDict(
    "Bosses Group" => Ahorn.EntityPlacement(BossesGroup)
)

function Ahorn.selection(entity::BossesGroup)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BossesGroup, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (1.0, 0.0, 0.0, 0.5), (1.0, 0.0, 0.0, 1.0))
end

end
