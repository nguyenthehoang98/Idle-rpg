using _KIT.Config;
using _KIT.Config.ExcelExtension.Runtime;

[ExcelAsset(
    ExcelPath = "Assets/Excels/LevelConfig.xlsx",
    ConfigPath = "Assets/_Sources/Configs/LevelConfig.asset")]
public class LevelConfig : KitBaseConfig
{
    public override void OnMapValue()
    {
    }
}