using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("Client")]
public class Success : ActionTask
{
    protected override void OnExecute()
    {
        base.OnExecute();
        EndAction(true);
    }
}