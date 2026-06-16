module MoltenLavafall

using ..Ahorn, Maple

@mapdef Entity "DZ/MoltenLavafall" MoltenLavafall(x::Integer, y::Integer, Player::Bool=true, flowSpeed::Number=60.0, height::Integer=64, width::Integer=8)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(MoltenLavafall),
    "wide" => Ahorn.EntityPlacement(MoltenLavafall)
)

function Ahorn.selection(entity::MoltenLavafall)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MoltenLavafall, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
