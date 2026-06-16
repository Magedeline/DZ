using System.Reflection;
using MonoMod.Utils;

namespace Celeste.Triggers 
{
    [CustomEntity(ids: "DZ/EntityReskinTrigger")]
    public class EntityReskinTrigger : Trigger {

        private readonly int entityIndex;
        private readonly string entityFullName;

        private readonly bool oneUse;
        private readonly string newSpriteID;


        public EntityReskinTrigger(EntityData data, Vector2 offset)
            : base(data, offset) {

            entityIndex = data.Int("entityIndex", -1);
            entityFullName = data.Attr("entityFullName", "CelesteStrawberry");

            oneUse = data.Bool("oneUse", true);
            newSpriteID = data.Attr("newSpriteID", "");
        }


        private List<Entity> entities = new();
        public override void Added(Scene scene) {
            base.Added(scene);

            int Index = entityIndex;
            int search = -1;
            foreach (Entity entity in Scene.Entities) {
                if (entity.GetType().FullName == entityFullName) {
                    search++;
                    if (Index < 0) {
                        entities.Add(entity);
                    } else if (search == Index) {
                        entities.Add(entity);
                        break;
                    }
                }
            }
        }

        public override void OnEnter(global::Celeste.Player player) {
            base.OnEnter(player);

            int Index = entityIndex;
            int search = -1;
            foreach (Entity entity in entities) {
                search++;
                if (Scene.Entities.Contains(entity)) {
                    Logger.Log(LogLevel.Debug, "DZ/EntityReskinTrigger", $"trying reskin Entity '{entity.GetType().FullName}' No.{(Index < 0 ? search : Index)}");
                    EntityReskin(entity, newSpriteID);
                }
            }
            if (oneUse) {
                Collidable = false;
            }
        }
        
        /// <summary>
        /// Gets the root path for animations in a sprite
        /// </summary>
        private static string getAnimationRootPath(Sprite sprite)
        {
            if (sprite?.Animations == null || sprite.Animations.Count == 0)
                return "";
            
            var firstAnim = sprite.Animations.Values.FirstOrDefault();
            if (firstAnim == null || firstAnim.Frames == null || firstAnim.Frames.Length == 0)
                return "";
                
            string path = firstAnim.Frames[0]?.AtlasPath ?? "";
            int lastSlash = path.LastIndexOf('/');
            return lastSlash >= 0 ? path.Substring(0, lastSlash + 1) : "";
        }
        
        public static void EntityReskin(Entity entity, string SpriteID) {

            string search = SpriteID;
            if (search.EndsWith("_")) { search = search.Substring(0, search.Length - 1); }

            if (SpriteID != null && GFX.SpriteBank.SpriteData.ContainsKey(search)) {
                Type entityType = entity.GetType();
                DynamicData entityData = new DynamicData(entity);
                Sprite sprite = entityData.Get<Sprite>("sprite");

                if (sprite != null) {
                    // --------sprite--------
                    sprite = GFX.SpriteBank.CreateOn(sprite, SpriteID);
                    string SpritePath = getAnimationRootPath(sprite);
                    // ----------------
                    // --------flash--------
                    Sprite flash = entityData.Get<Sprite>("flash");
                    if (flash != null) {
                        GFX.SpriteBank.CreateOn(flash, SpriteID);
                    }
                    // ----------------
                    // --------outline--------
                    var Field_outline = entityType.GetField("outline", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (Field_outline != null && Field_outline.GetValue(entity) is Entity outline) {
                        foreach (Component component in outline) {
                            if (component is Image image && GFX.Game.Has($"{SpritePath}outline")) {
                                image.Texture = GFX.Game[$"{SpritePath}outline"];
                            }
                        }
                    } else if (Field_outline != null && Field_outline.GetValue(entity) is Image image) {
                        if (GFX.Game.Has($"{SpritePath}outline")) {
                            image.Texture = GFX.Game[$"{SpritePath}outline"];
                        }
                    }
                    // ----------------
                    // --------particle--------
                    if (entity is Booster || entity is Cloud) {
                        var Field_particle = entityType.GetField("particleType", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                        if (Field_particle != null && Field_particle.GetValue(entity) is ParticleType particleType) {
                            // Clone object to prevent lost of vanilla
                            particleType = new(particleType);
                            Field_particle.SetValue(entity, particleType);
                            //

                            string particle = "blob";
                            if (entity is Cloud) {
                                particle = "clouds";
                            }

                            if (GFX.Game.Has(SpritePath + particle)) {
                                particleType.Source = GFX.Game[SpritePath + particle];
                                particleType.Color = Color.White;
                            }
                        }
                    }
                    // ----------------
                } else {
                    Logger.Log(LogLevel.Warn, "DZ/EntityReskinTrigger", $"Entity '{entity.GetType().FullName}' Not compatible with this trigger");
                }
            } else {
                Logger.Log(LogLevel.Warn, "DZ/EntityReskinTrigger", $"'{search}' is not defined in Sprites.xml!");
            }
        }

    }
}
