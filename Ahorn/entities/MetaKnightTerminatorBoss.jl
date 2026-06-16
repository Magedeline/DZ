module MetaKnightTerminatorBoss

using ..Ahorn, Maple

@mapdef Entity "DZ/MetaKnightTerminatorBoss" MetaKnightTerminatorBoss(x::Integer, y::Integer, health::Integer=400, maxHealth::Integer=400)

const placements = Ahorn.PlacementDict(
    "meta_knight_terminator_boss" => Ahorn.EntityPlacement(MetaKnightTerminatorBoss),
    "digital_king_ddd_boss" => Ahorn.EntityPlacement(MetaKnightTerminatorBoss),
    "martlet_bird_possess_boss" => Ahorn.EntityPlacement(MetaKnightTerminatorBoss),
    "black_dark_matter_boss" => Ahorn.EntityPlacement(MetaKnightTerminatorBoss),
    "dark_matter_knife_boss" => Ahorn.EntityPlacement(MetaKnightTerminatorBoss)
)

function Ahorn.selection(entity::MetaKnightTerminatorBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MetaKnightTerminatorBoss, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/metaknight/mk_idle00", entity.x, entity.y)
end

end
