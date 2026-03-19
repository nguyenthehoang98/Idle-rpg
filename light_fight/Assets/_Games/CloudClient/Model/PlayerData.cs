using CloudAPI.Model;

[System.Serializable]
public class PlayerData : IConvertObjectData
{
    public string Name;
    public string Local;
    
    public string ObjectTitle() => "PlayerData";
}