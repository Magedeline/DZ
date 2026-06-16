#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Bosses;

[CustomEntity("DZ/ELSTerminaHealth")]
[Tracked(false)]
public class ELSTerminaHealth : Entity
{
    private Sprite? baseBar;
    private List<HealthSlice> healthSlices;
    private Level level = null!;
    private float currentHealth;
    private float maxHealth;
    private int sliceCount;
    private float sliceWidth;

    public ELSTerminaHealth(float maxHealth, bool hardMode)
    {
        this.maxHealth = maxHealth;
        this.currentHealth = maxHealth;
        
        // Calculate number of slices based on max health
        this.sliceCount = hardMode ? 500 : 250;
        this.sliceWidth = hardMode ? 1.2f : 2.4f;

        this.healthSlices = new List<HealthSlice>();
        this.Tag = Tags.HUD | Tags.PauseUpdate;
        Depth = -100000;

        // Create base bar sprite (placeholder - replace with actual sprite)
        this.baseBar = new Sprite(GFX.Gui, "heartgem/0/0");
        this.baseBar.CenterOrigin();
        this.Add(this.baseBar);

        // Create health slices
        for (int i = 0; i < this.sliceCount; i++)
        {
            this.healthSlices.Add(new HealthSlice(hardMode));
        }
    }

    public ELSTerminaHealth(EntityData data, Vector2 offset)
        : this(data.Float("maxHealth", 300f), data.Bool("hardMode", false))
    {
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.level = SceneAs<Level>();
        Position = new Vector2(960f, 100f);
        Add(new Coroutine(CreateBar()));
    }

    private IEnumerator CreateBar()
    {
        // Animate base bar appearing
        if (this.baseBar != null)
        {
            this.baseBar.Scale = Vector2.Zero;
            for (float t = 0f; t < 1f; t += Engine.DeltaTime * 2f)
            {
                if (this.baseBar != null)
                    this.baseBar.Scale = Vector2.One * Ease.ElasticOut(t);
                yield return null;
            }
            if (this.baseBar != null)
                this.baseBar.Scale = Vector2.One;
        }

        // Create health slices
        float totalWidth = this.sliceCount * this.sliceWidth;
        float startX = Position.X - totalWidth / 2f + this.sliceWidth / 2f;

        for (int i = 0; i < this.sliceCount; i++)
        {
            if (i < this.healthSlices.Count)
            {
                HealthSlice slice = this.healthSlices[i];
                this.level.Add(slice);
                slice.Position = new Vector2(startX + i * this.sliceWidth, Position.Y + 20f);
                slice.Appear();
                yield return 0.02f;
            }
        }
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        this.currentHealth = currentHealth;
        this.maxHealth = maxHealth;

        // Calculate how many slices should be visible
        float healthPercent = currentHealth / maxHealth;
        int visibleSlices = (int)Math.Ceiling(this.sliceCount * healthPercent);

        // Remove slices if health decreased
        while (this.healthSlices.Count > visibleSlices && this.healthSlices.Count > 0)
        {
            int lastIndex = this.healthSlices.Count - 1;
            this.healthSlices[lastIndex].Remove();
            this.healthSlices.RemoveAt(lastIndex);
        }
    }

    public override void Render()
    {
        base.Render();

        // Draw base bar background
        if (this.baseBar != null)
        {
            float totalWidth = this.sliceCount * this.sliceWidth + 20f;
            Draw.Rect(Position.X - totalWidth / 2f, Position.Y, totalWidth, 40f, Color.Black * 0.7f);
            Draw.HollowRect(Position.X - totalWidth / 2f, Position.Y, totalWidth, 40f, Color.Purple * 0.8f);
        }

        // Draw boss name
        string bossName = "ELS TERMINA";
        Vector2 textSize = Draw.DefaultFont.MeasureString(bossName);
        Draw.SpriteBatch.DrawString(Draw.DefaultFont, bossName, Position - textSize / 2f + new Vector2(0f, -30f), Color.White);
    }

    private class HealthSlice : Entity
    {
        private Sprite? sprite;
        private bool removing;

        public HealthSlice(bool small)
        {
            this.removing = false;
            
            // Placeholder sprite - replace with actual health slice sprite
            this.sprite = new Sprite(GFX.Gui, "heartgem/0/0");
            this.sprite.CenterOrigin();
            this.Add(this.sprite);

            // Scale based on size (adjusted for smaller slice widths)
            float scale = small ? 0.15f : 0.3f;
            this.sprite.Scale = Vector2.One * scale;

            this.Tag = Tags.HUD | Tags.PauseUpdate;
            Depth = -100001;
        }

        public void Appear()
        {
            if (this.sprite != null)
            {
                this.sprite.Scale = Vector2.Zero;
                Add(new Coroutine(AppearCoroutine()));
            }
        }

        private IEnumerator AppearCoroutine()
        {
            if (this.sprite == null) yield break;

            float targetScale = this.sprite.Scale.X > 0.2f ? 0.3f : 0.15f;
            for (float t = 0f; t < 1f; t += Engine.DeltaTime * 4f)
            {
                this.sprite.Scale = Vector2.One * targetScale * Ease.BackOut(t);
                yield return null;
            }
            this.sprite.Scale = Vector2.One * targetScale;
        }

        public void Remove()
        {
            if (this.removing) return;
            this.removing = true;
            Add(new Coroutine(RemoveCoroutine()));
        }

        private IEnumerator RemoveCoroutine()
        {
            if (this.sprite == null) yield break;

            float startScale = this.sprite.Scale.X;
            for (float t = 0f; t < 1f; t += Engine.DeltaTime * 4f)
            {
                if (this.sprite != null)
                    this.sprite.Scale = Vector2.One * startScale * (1f - Ease.CubeIn(t));
                yield return null;
            }
            RemoveSelf();
        }

        public override void Render()
        {
            base.Render();

            // Draw health slice
            if (this.sprite != null)
            {
                this.sprite.Color = Color.Lerp(Color.Purple, Color.Red, 0.3f);
                this.sprite.Render();
            }
        }
    }
}
