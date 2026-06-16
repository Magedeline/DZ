module MusicCartridge

using ..Ahorn, Maple

@mapdef Entity "Maggy_DesoloZantas/MusicCartridge" MusicCartridge(x::Integer, y::Integer, cartridgeId::String="music_default", color::String="FF1493", label::String="???", musicEvent::String="", name::String="Music Cartridge", persistent::Bool=true, playOnCollect::Bool=true, remixIndex::Integer=0, unlockFlag::String="cartridge_music_default")

const placements = Ahorn.PlacementDict(
    "music_cartridge" => Ahorn.EntityPlacement(MusicCartridge),
    "Music Cartridge" => Ahorn.EntityPlacement(MusicCartridge)
)

function Ahorn.selection(entity::MusicCartridge)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MusicCartridge, room::Maple.Room)
    Ahorn.drawSprite(ctx, "collectibles/Maggy_DesoloZantas/musiccartridge/idle00", entity.x, entity.y)
end

end
