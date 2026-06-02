module MoltenLava

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/MoltenLava" MoltenLava(x::Integer, y::Integer, Player::Bool=true, damageGracePeriod::Number=0.0, hasBottom::Bool=false, hasTop::Bool=true, height::Integer=32, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(MoltenLava),
    "deep" => Ahorn.EntityPlacement(MoltenLava)
)

function Ahorn.selection(entity::MoltenLava)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MoltenLava, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
