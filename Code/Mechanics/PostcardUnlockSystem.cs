using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste;

/// <summary>
/// Handles showing postcards when unlocking C-Side, D-Side, and DX-Side.
/// Modeled after Celeste's vanilla B-Side unlock postcard but with custom 
/// textures and sound events for each side.
/// </summary>
public static class PostcardUnlockSystem
{
    private const string DefaultPostcardTexture = "MaggyHelper/postcard";

    // Ã¢â€â‚¬Ã¢â€â‚¬ Postcard Configuration Per Side Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬

    /// <summary>
    /// Configuration for each side's unlock postcard.
    /// </summary>
    public class PostcardConfig
    {
        public string DialogKey { get; set; }
        public string TexturePath { get; set; }
        public string SfxIn { get; set; }
        public string SfxOut { get; set; }
        public Color TintColor { get; set; }
        public string UnlockMusic { get; set; }
    }

    /// <summary>Postcard config for C-Side unlock (shown after completing B-Side)</summary>
    public static readonly PostcardConfig CSideConfig = new()
    {
        DialogKey = "POSTCARD_CSIDE_UNLOCK",
        TexturePath = "postcards/cside_unlock",
        SfxIn = "event:/ui/pusheen/main/postcard_csides_in",
        SfxOut = "event:/ui/pusheen/main/postcard_csides_out",
        TintColor = new Color(255, 215, 0),  // Gold tint
        UnlockMusic = "event:/music/pusheen/menu/complete_cside"
    };

    /// <summary>Postcard config for D-Side unlock (shown after completing C-Side)</summary>
    public static readonly PostcardConfig DSideConfig = new()
    {
        DialogKey = "POSTCARD_DSIDE_UNLOCK",
        TexturePath = "postcards/dside_unlock",
        SfxIn = "event:/ui/pusheen/main/postcard_dsides_in",
        SfxOut = "event:/ui/pusheen/main/postcard_dsides_out",
        TintColor = new Color(180, 100, 255),  // Rainbow/purple tint
        UnlockMusic = "event:/music/pusheen/menu/complete_cside_summit"
    };

    /// <summary>Postcard config for DX-Side unlock (shown after completing D-Side)</summary>
    public static readonly PostcardConfig DXSideConfig = new()
    {
        DialogKey = "POSTCARD_DXSIDE_UNLOCK",
        TexturePath = "postcards/dxside_unlock",
        SfxIn = "event:/ui/pusheen/main/postcard_dsides_in",
        SfxOut = "event:/ui/pusheen/main/postcard_dsides_out",
        TintColor = new Color(50, 0, 80),  // Dark void tint
        UnlockMusic = "event:/music/pusheen/menu/complete_cside_summit"
    };

    /// <summary>Postcard config for the 100% ultra completion postcard.</summary>
    public static readonly PostcardConfig UltraVariantConfig = new()
    {
        DialogKey = "POSTCARD_ULTRA_VARIANT_UNLOCK",
        TexturePath = "postcards/ultra_variant_unlock",
        SfxIn = "event:/new_content/ui/pusheen/postcard_desolo_variants_in",
        SfxOut = "event:/new_content/ui/pusheen/postcard_desolo_variants_out",
        TintColor = new Color(255, 160, 220),
        UnlockMusic = "event:/music/pusheen/menu/complete_cside_summit"
    };

    // Ã¢â€â‚¬Ã¢â€â‚¬ Postcard Display Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬

    /// <summary>
    /// Gets the postcard config for a side unlock based on which side was just completed.
    /// </summary>
    public static PostcardConfig GetUnlockConfig(int completedMode)
    {
        return completedMode switch
        {
            AreaModeExtender.MODE_BSIDE => CSideConfig,   // Completing B unlocks C
            AreaModeExtender.MODE_CSIDE => DSideConfig,   // Completing C unlocks D
            AreaModeExtender.MODE_DSIDE => DXSideConfig,  // Completing D unlocks DX
            _ => null
        };
    }

    /// <summary>
    /// Creates and displays a postcard for unlocking a new side.
    /// Called from the LevelExit flow after completing a side.
    /// </summary>
    public static IEnumerator ShowUnlockPostcard(Scene scene, Session session, int completedMode)
    {
        var config = GetUnlockConfig(completedMode);
        if (config == null)
            yield break;

        // Get the dialog text for this postcard
        string dialogText = Dialog.Get(config.DialogKey);
        if (string.IsNullOrEmpty(dialogText))
        {
            // Fallback text
            string sideName = completedMode switch
            {
                AreaModeExtender.MODE_BSIDE => "C-Side",
                AreaModeExtender.MODE_CSIDE => "D-Side",
                AreaModeExtender.MODE_DSIDE => "DX-Side",
                _ => "New Side"
            };
            dialogText = $"{sideName} Unlocked!\nComplete the challenge to earn a new heart gem.";
        }

        // Create and display the postcard
        var postcard = new PostcardMaggy(dialogText, config.SfxIn, config.SfxOut);

        // Try to load custom postcard texture
        TryApplyPostcardTexture(postcard, config.TexturePath, DefaultPostcardTexture);

        scene.Add(postcard);

        // Play unlock music
        if (!string.IsNullOrEmpty(config.UnlockMusic))
        {
            Audio.SetMusic(config.UnlockMusic);
        }

        // Display the postcard routine
        yield return postcard.DisplayRoutine();

        // Mark the next side as unlocked in save data
        MarkSideUnlocked(session, completedMode + 1);

        Logger.Log(LogLevel.Info, "MaggyHelper",
            $"Postcard shown for completing mode {completedMode}, unlocking mode {completedMode + 1}");
    }

    /// <summary>
    /// Displays the ultra completion postcard when the save reaches 100%.
    /// </summary>
    public static IEnumerator ShowUltraCompletionPostcard(Scene scene)
    {
        var config = UltraVariantConfig;
        string dialogText = Dialog.Get(config.DialogKey);
        if (string.IsNullOrEmpty(dialogText))
        {
            dialogText = "Ultra variant unlocked! 100% completion reached.";
        }

        var postcard = new PostcardMaggy(dialogText, config.SfxIn, config.SfxOut);

        TryApplyPostcardTexture(postcard, config.TexturePath, DefaultPostcardTexture);

        scene.Add(postcard);

        if (!string.IsNullOrEmpty(config.UnlockMusic))
            Audio.SetMusic(config.UnlockMusic);

        yield return postcard.DisplayRoutine();
    }

    private static void TryApplyPostcardTexture(PostcardMaggy postcard, params string[] texturePaths)
    {
        try
        {
            foreach (string texturePath in texturePaths)
            {
                if (!string.IsNullOrWhiteSpace(texturePath) && GFX.Gui.Has(texturePath))
                {
                    postcard.Postcard = GFX.Gui[texturePath];
                    return;
                }
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// Marks a side as unlocked in the save data.
    /// </summary>
    private static void MarkSideUnlocked(Session session, int newMode)
    {
        if (session == null || newMode >= AreaModeExtender.TOTAL_MODES)
            return;

        string unlockKey = $"{AreaData.Get(session.Area)?.SID}_{AreaModeExtender.GetModeName(newMode)}_unlocked";
        global::Celeste.Mod.MaggyHelper.MaggyHelperModule.SaveData?.UnlockAchievement(unlockKey);
    }
}

/// <summary>
/// Custom unlock postcard vignette that plays between level completion and the 
/// overworld return, similar to how vanilla Celeste shows the B-Side tape card.
/// </summary>
public class SideUnlockVignette : Scene
{
    private readonly Session session;
    private readonly int completedMode;
    private HiresSnow snow;
    private bool started;

    public SideUnlockVignette(Session session, int completedMode)
    {
        this.session = session;
        this.completedMode = completedMode;
    }

    public override void Begin()
    {
        base.Begin();
        snow = new HiresSnow();
        Add(snow);
    }

    public override void Update()
    {
        base.Update();

        if (!started)
        {
            started = true;
            var entity = new Entity();
            entity.Add(new Coroutine(Routine()));
            Add(entity);
        }
    }

    private IEnumerator Routine()
    {
        // Brief delay before showing postcard
        yield return 0.5f;

        // Show the unlock postcard
        yield return PostcardUnlockSystem.ShowUnlockPostcard(this, session, completedMode);

        // Transition back to overworld
        yield return 0.5f;
        Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete, snow);
    }
}

