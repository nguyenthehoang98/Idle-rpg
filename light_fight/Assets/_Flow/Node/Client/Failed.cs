using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("Client")]
public class Failed : ActionTask
{
   protected override void OnExecute()
   {
      base.OnExecute();
      EndAction(false);
   }
}