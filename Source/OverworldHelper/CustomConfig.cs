using System;
using System.Linq;
using System.Text;
using Celeste.Mod.Meta;
using YamlDotNet.Serialization;

namespace Celeste.Mod.DZ;

public class CustomConfig
{
    private static IDeserializer deserializer => new DeserializerBuilder().Build();

    private static MapMeta DeserializeConfig(string yamlData, Type ty)
    {
        return (MapMeta)deserializer.Deserialize(yamlData, ty);
    }

    public static MapMeta GetConfig(string areaLevelSet, string areaSID, Type ty)
    {
        if (!(typeof(MapMeta).IsAssignableFrom(ty))) throw new ArgumentException("given config type must inherit from MapMeta class");
        ModAsset levelSetAsset = Everest.Content.Get("Maps/"+areaLevelSet);
        if (levelSetAsset != null)
        {
            ModAsset areaMeta = levelSetAsset.Children.Find((ModAsset asset) =>
            {
                return asset.PathVirtual.EndsWith(areaSID+".meta");
            });
            if (areaMeta != null)
            {
                string confData = Encoding.UTF8.GetString(areaMeta.Data);
                if (confData.Length > 0 && confData[0] == '\uFEFF')
                    confData = confData.Substring(1);
                return DeserializeConfig(confData, ty);
            }
        }
        return null;
    }
    
    public static MapMeta GetConfig(AreaKey area, Type ty) => GetConfig(area.GetLevelSet(), area.GetSID(), ty);

    public static MapMeta GetConfig(string area, Type ty)
    {
        if (!area.Contains('/')) return null;
        string levelSet = string.Join("/",area.Split("/").SkipLast(1));
        return GetConfig(levelSet,area,ty);
    }
}