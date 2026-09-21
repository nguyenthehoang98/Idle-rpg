using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using _TDS.Gameplay;

namespace _TDS.Tests.Editor
{
    /// <summary>
    /// GameplayScene phải có WaveUpgradePanel được wire sẵn, nếu không sẽ log error lúc Awake
    /// và bỏ qua toàn bộ luồng chọn upgrade giữa wave.
    /// </summary>
    public class GameplaySceneWiringTests
    {
        [Test]
        public void GameplaySceneHasConfiguredWaveUpgradePanel()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/GameplayScene.unity", OpenSceneMode.Single);

            GameplayScene gameplay = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<GameplayScene>(true))
                .FirstOrDefault();

            Assert.That(gameplay, Is.Not.Null, "GameplayScene component not found in the scene");

            WaveUpgradePanel panel = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<WaveUpgradePanel>(true))
                .FirstOrDefault();

            Assert.That(panel, Is.Not.Null, "WaveUpgradePanel chưa có trong scene - chạy TDS/Configure Gameplay UI");
            Assert.That(panel.IsConfigured, Is.True, "WaveUpgradePanel thiếu UI reference - chạy TDS/Configure Gameplay UI");
        }
    }
}
