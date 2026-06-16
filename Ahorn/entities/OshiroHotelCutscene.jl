module OshiroHotelCutscene

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/OshiroHotelCutscene" OshiroHotelCutscene(x::Integer, y::Integer, phase::String="front_desk")

const placements = Ahorn.PlacementDict(
    "Oshiro Front Desk" => Ahorn.EntityPlacement(OshiroHotelCutscene),
    "Oshiro Hallway A" => Ahorn.EntityPlacement(OshiroHotelCutscene),
    "Oshiro Hallway B" => Ahorn.EntityPlacement(OshiroHotelCutscene),
    "Oshiro Clutter" => Ahorn.EntityPlacement(OshiroHotelCutscene),
    "Hotel Guestbook" => Ahorn.EntityPlacement(OshiroHotelCutscene),
    "Hotel Memo" => Ahorn.EntityPlacement(OshiroHotelCutscene)
)

function Ahorn.selection(entity::OshiroHotelCutscene)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::OshiroHotelCutscene, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/oshiro/idle00", entity.x, entity.y)
end

# Nodes: min=0, max=inf
# Basic node rendering not implemented in auto-generated plugin

end
