using System.Linq;
using UnityEditor;

namespace _Game.Battle.Editor
{
    public class LevelSpawnValidate
    {
        [MenuItem("Tools/Validate/Level Spawn")]
        static void Validate()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(LevelSpawnConfig).Name}");
            LevelSpawnConfig[] array = guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<LevelSpawnConfig>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();
            foreach (var item in array)
            {
                item.Validate();
            }
        }
    }
}