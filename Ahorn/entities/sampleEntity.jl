module SampleEntity

using ..Ahorn, Maple

@mapdef Entity "DZ/SampleEntity" SampleEntity(x::Integer, y::Integer, sampleProperty::Integer=0)

const placements = Ahorn.PlacementDict(
    "DZ/SampleEntity" => Ahorn.EntityPlacement(SampleEntity)
)

function Ahorn.selection(entity::SampleEntity)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SampleEntity, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/Ingeste/sampleEntity/idle00", entity.x, entity.y)
end

end
