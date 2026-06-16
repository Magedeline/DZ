module FakeHeartGem

using ..Ahorn, Maple

@mapdef Entity "DZ/FakeHeartGem" FakeHeartGem(x::Integer, y::Integer, collectMessage::String="It's fake!", persistent::Bool=false, respawnTime::Number=3.0)

const placements = Ahorn.PlacementDict(
    "fakegemheart" => Ahorn.EntityPlacement(FakeHeartGem),
    "persistent" => Ahorn.EntityPlacement(FakeHeartGem)
)

function Ahorn.selection(entity::FakeHeartGem)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FakeHeartGem, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
