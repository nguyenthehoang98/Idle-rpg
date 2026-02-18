namespace _Game.Battle
{
    public interface IVisitor
    {
        void Visit(int entity);
        void VisitCell(int x, int y);
    }
}