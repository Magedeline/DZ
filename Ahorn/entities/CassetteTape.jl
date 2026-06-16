module CassetteTape

using ..Ahorn, Maple

@mapdef Entity "DZ/CassetteTape" CassetteTape(x::Integer, y::Integer, audioEvent::String="", autoPlay::Bool=false, color::String="FFA500", description::String="A mysterious cassette tape.", displayName::String="Cassette Tape", oneTimeUse::Bool=false, remixIndex::Integer=0, tapeId::String="tape_default")

const placements = Ahorn.PlacementDict(
    "cassette_tape" => Ahorn.EntityPlacement(CassetteTape),
    "cassette_tape_auto" => Ahorn.EntityPlacement(CassetteTape)
)

function Ahorn.selection(entity::CassetteTape)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CassetteTape, room::Maple.Room)
    Ahorn.drawSprite(ctx, "collectibles/desolozantas/cassettetape/idle00", entity.x, entity.y)
end

end
