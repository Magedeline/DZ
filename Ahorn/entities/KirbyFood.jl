module KirbyFood

using ..Ahorn, Maple

@mapdef Entity "DZ/KirbyFood" KirbyFood(x::Integer, y::Integer, despawnTime::Integer=10, foodType::Integer=0, healAmount::Integer=0, isDamaging::Bool=false, isFalling::Bool=false)

const placements = Ahorn.PlacementDict(
    "Apple" => Ahorn.EntityPlacement(KirbyFood),
    "Max Tomato (Full Heal)" => Ahorn.EntityPlacement(KirbyFood),
    "Meat" => Ahorn.EntityPlacement(KirbyFood),
    "Cake" => Ahorn.EntityPlacement(KirbyFood),
    "Cherry Bunch" => Ahorn.EntityPlacement(KirbyFood),
    "Invincibility Star" => Ahorn.EntityPlacement(KirbyFood),
    "1-Up" => Ahorn.EntityPlacement(KirbyFood),
    "Custom Food" => Ahorn.EntityPlacement(KirbyFood),
    "Damaging Apple (Boss Drop)" => Ahorn.EntityPlacement(KirbyFood)
)

function Ahorn.selection(entity::KirbyFood)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbyFood, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
