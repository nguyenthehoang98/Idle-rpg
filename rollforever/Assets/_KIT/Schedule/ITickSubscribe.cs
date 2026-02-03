namespace _KIT.Schedule
{
    public interface ITickSubscribe
    {
        void Register(ITick module);
        void UnRegister(ITick module);
    }
}