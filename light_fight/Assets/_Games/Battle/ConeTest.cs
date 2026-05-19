using _Games.Battle;
using UnityEngine;

public class ConeTest : MonoBehaviour
{
    public BattleLevel battleLevel;

    private bool needUpdatePosition = false;
    private Weapon currentWeapon;
    private Cone currentCone;

    async void Start()
    {
        Application.targetFrameRate = 30;
        await battleLevel.Initialize(1, 1);
        battleLevel.Play();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) PickCone(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) PickCone(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) PickCone(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) PickCone(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) PickCone(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) PickCone(5);

        if (Input.GetKeyDown(KeyCode.Mouse0) && currentWeapon != null)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;
            currentWeapon.RotateTo(worldPos, needUpdatePosition, null);
            needUpdatePosition = false;
        }
    }

    void PickCone(int index)
    {
        for (int i = 0; i < 6; i++)
        {
            if (i == index)
            {
                needUpdatePosition = true;
                currentCone = battleLevel.GetCone(i);
                currentCone.Active();
                currentWeapon = currentCone.Weapon;
            }
            else
            {
                battleLevel.GetCone(i).Inactive();
            }
        }
    }
}
