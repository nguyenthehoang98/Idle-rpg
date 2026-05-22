using UnityEngine;

namespace _Games.Battle
{
    [ExecuteInEditMode, ExecuteAlways]
    public class ConeMonoTest : MonoBehaviour
    {
        private Cone[] cones;
        private bool needUpdatePosition = false;

        private Weapon currentWeapon;
        private Cone currentCone;

        private void Awake()
        {
            cones = new Cone[BattleConst.MAX_DICE_NUMBER];
            for (int i = 0; i < cones.Length; i++)
            {
                cones[i] = new Cone(i, Vector3.zero);
            }
        }

        private void OnValidate() => Awake();

        private void Update()
        {
            if (cones == null) Awake();
            if (cones == null) return;

            for (int i = 0; i < cones.Length; i++)
            {
                cones[i].Draw();
            }

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
                    currentCone = cones[i];
                    currentCone.Active();
                    currentWeapon = currentCone.Weapon;
                }
                else
                {
                    if (cones[i].IsPlaying) cones[i].Inactive();
                }
            }
        }
    }
}