using CloudAPI.Model;

[System.Serializable]
public class ProgressData : IConvertObjectData
{
    public int Chapter;
    public int Power;
    
    public string ObjectTitle() => "ProgressData";
}