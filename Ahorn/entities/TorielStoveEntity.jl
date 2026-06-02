module TorielStoveEntity

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/TorielStoveEntity" TorielStoveEntity(x::Integer, y::Integer, canInteract::Bool=true, cookDuration::Number=5.0, dialogueId::String="TORIEL_STOVE", hasPie::Bool=true, healAmount::Integer=3)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(TorielStoveEntity)
)

function Ahorn.selection(entity::TorielStoveEntity)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TorielStoveEntity, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
