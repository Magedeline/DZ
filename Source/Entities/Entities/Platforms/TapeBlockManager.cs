using System.Runtime.CompilerServices;
using global::Celeste.Mod.Meta;
using FMOD.Studio;
using Celeste.Entities;

namespace DZ;

[Tracked(true)]
[HotReloadable]
public class TapeBlockManager : Entity
{
    private int currentIndex;

    private float beatTimer;

    private int beatIndex;

    private float tempoMult;

    private int leadBeats;

    private int maxBeat;

    private bool isLevelMusic;

    private int beatIndexOffset;

    private EventInstance sfx;

    private EventInstance sfxPerc;

    private EventInstance snapshot;

    private int beatsPerTick;

    private int ticksPerSwap;

    private int beatIndexMax;

    public TapeBlockManager()
    {
        base.Tag = Tags.Global;
        Add(new TransitionListener
        {
            OnOutBegin = () =>
            {
                if (!SceneAs<Level>().HasCassetteBlocks)
                {
                    RemoveSelf();
                }
                else
                {
                    maxBeat = SceneAs<Level>().CassetteBlockBeats;
                    tempoMult = SceneAs<Level>().CassetteBlockTempo;
                }
            }
        });
    }
    public override void Awake(Scene scene)
    {
        AreaData areaData = AreaData.Get(scene);
        if (areaData.CassetteSong == "-" || string.IsNullOrWhiteSpace(areaData.CassetteSong))
        {
            areaData.CassetteSong = null;
        }
        orig_Awake(scene);
        beatsPerTick = 4;
        ticksPerSwap = 2;
        beatIndexMax = 256;
        MapMetaCassetteModifier MapMetaCassetteModifier = areaData.Meta?.CassetteModifier;
        if (MapMetaCassetteModifier == null)
        {
            return;
        }
        if (MapMetaCassetteModifier.OldBehavior)
        {
            tempoMult = MapMetaCassetteModifier.TempoMult;
            maxBeat = MapMetaCassetteModifier.Blocks;
        }
        leadBeats = MapMetaCassetteModifier.LeadBeats;
        beatsPerTick = MapMetaCassetteModifier.BeatsPerTick;
        ticksPerSwap = MapMetaCassetteModifier.TicksPerSwap;
        beatIndexMax = MapMetaCassetteModifier.BeatsMax;
        beatIndexOffset = MapMetaCassetteModifier.BeatIndexOffset;
        TransitionListener transitionListener = Get<TransitionListener>();
        if (transitionListener == null)
        {
            return;
        }
        transitionListener.OnOut = delegate
        {
            if (base.Scene != null)
            {
                Update();
            }
        };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        if (!isLevelMusic)
        {
            Audio.Stop(snapshot);
            Audio.Stop(sfx);
            Audio.Stop(sfxPerc);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
        if (!isLevelMusic)
        {
            Audio.Stop(snapshot);
            Audio.Stop(sfx);
            Audio.Stop(sfxPerc);
        }
    }

    public override void Update()
    {
        base.Update();
        if (isLevelMusic)
        {
            sfx = Audio.CurrentMusicEventInstance;
        }
        if (sfx == null && !isLevelMusic)
        {
            string CassetteSong = AreaData.Areas[SceneAs<Level>().Session.Area.ID].CassetteSong;
            sfx = Audio.CreateInstance(CassetteSong);
            sfxPerc = Audio.CreateInstance("event:/music/pusheen/Cassette/tape/tape_perc");
            if (leadBeats == 0)
            {
                beatIndex = 0;
                sfx?.start();
                sfxPerc?.start();
            }
        }
        else
        {
            AdvanceMusic(Engine.DeltaTime * tempoMult);
        }
    }

    public void AdvanceMusic(float time)
    {
        beatTimer += time;
        if (beatTimer < 1f / 6f)
        {
            return;
        }
        beatTimer -= 1f / 6f;
        beatIndex++;
        beatIndex %= beatIndexMax;
        if (beatIndex % (beatsPerTick * ticksPerSwap) == 0)
        {
            currentIndex++;
            currentIndex %= maxBeat;
            SetActiveIndex(currentIndex);
            if (!isLevelMusic)
            {
                Audio.Play("event:/music/pusheen/Cassette/tape/tape_perc");
            }
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
        }
        else
        {
            if ((beatIndex + 1) % (beatsPerTick * ticksPerSwap) == 0)
            {
                SetWillActivate((currentIndex + 1) % maxBeat);
            }
            if (beatIndex % beatsPerTick == 0 && !isLevelMusic)
            {
                Audio.Play("event:/music/pusheen/Cassette/tape/tape_perc");
            }
        }
        if (leadBeats > 0)
        {
            leadBeats--;
            if (leadBeats == 0)
            {
                beatIndex = 0;
                if (!isLevelMusic)
                {
                    sfx?.start();
                    sfxPerc?.start();
                }
            }
        }
        if (leadBeats <= 0)
        {
            sfxPerc?.setParameterValue("sixteenth_note_tape", GetSixteenthNote());
        }
    }

    public int GetSixteenthNote()
    {
        return (beatIndex + beatIndexOffset) % beatIndexMax + 1;
    }

    public void StopBlocks()
    {
        foreach (TapeBlock entity in base.Scene.Tracker.GetEntities<TapeBlock>())
        {
            entity.Finish();
        }
        if (!isLevelMusic)
        {
            Audio.Stop(sfx);
            Audio.Stop(sfxPerc);
        }
    }
    public void Finish()
    {
        if (!isLevelMusic)
        {
            Audio.Stop(snapshot);
        }
        RemoveSelf();
    }

    public void OnLevelStart()
    {
        Level level = base.Scene as Level;
        MapMetaCassetteModifier MapMetaCassetteModifier = AreaData.Get(level.Session).Meta?.CassetteModifier;
        if (MapMetaCassetteModifier != null && MapMetaCassetteModifier.OldBehavior)
        {
            currentIndex = maxBeat - 1 - beatIndex / beatsPerTick % maxBeat;
        }
        else
        {
            maxBeat = level.CassetteBlockBeats;
            tempoMult = level.CassetteBlockTempo;
            if (beatIndex % (beatsPerTick * ticksPerSwap) > beatsPerTick * ticksPerSwap / 2)
            {
                currentIndex = maxBeat - 2;
            }
            else
            {
                currentIndex = maxBeat - 1;
            }
        }
        SilentUpdateBlocks();
    }

    private void SilentUpdateBlocks()
    {
        foreach (TapeBlock entity in base.Scene.Tracker.GetEntities<TapeBlock>())
        {
            if (entity.ID.Level == SceneAs<Level>().Session.Level)
            {
                entity.SetActivatedSilently(entity.Index == currentIndex);
            }
        }
    }

    public void SetActiveIndex(int index)
    {
        foreach (TapeBlock entity in base.Scene.Tracker.GetEntities<TapeBlock>())
        {
            entity.Activated = entity.Index == index;
        }
    }

    public void SetWillActivate(int index)
    {
        foreach (TapeBlock entity in base.Scene.Tracker.GetEntities<TapeBlock>())
        {
            if (entity.Index == index || entity.Activated)
            {
                entity.WillToggle();
            }
        }
    }
    public void orig_Awake(Scene scene)
    {
        base.Awake(scene);
        isLevelMusic = AreaData.Areas[SceneAs<Level>().Session.Area.ID].CassetteSong == null;
        if (isLevelMusic)
        {
            leadBeats = 0;
            beatIndexOffset = 5;
        }
        else
        {
            beatIndexOffset = 0;
            leadBeats = 16;
            snapshot = Audio.CreateSnapshot("snapshot:/maggy_music_mains_mute");
        }
        maxBeat = SceneAs<Level>().CassetteBlockBeats;
        tempoMult = SceneAs<Level>().CassetteBlockTempo;
    }
}




