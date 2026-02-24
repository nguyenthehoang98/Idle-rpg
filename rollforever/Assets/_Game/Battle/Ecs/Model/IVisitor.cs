namespace _Game.Battle.Ecs.Model
{
    public interface IVisitor
    {
        void Visit(int entity);
        void VisitCell(int x, int y);
    }
}