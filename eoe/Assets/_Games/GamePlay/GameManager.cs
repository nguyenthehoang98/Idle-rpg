using _Games.GamePlay.AnimationSystem;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using UnityEngine;

namespace _Games.GamePlay
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private TickSystemOwner owner;
        [SerializeField] private Pedestal pedestal;
        [SerializeField] private Character[] characters;

        private void Start()
        {
            for (int i = 0; i < 7; i++)
            {
                if (i < characters.Length)
                {
                    Character character = characters[i];
                    if (character != null) character.Animator.Play(State.Idle, Direction.B);
                }

                Deactivate(i, 0);
            }
            
            AssetBundleManager.SetLocationBundle(true);
            owner.Initialize();
        }

        private void Update()
        {
            float duration = 0.5f;

            if (Input.GetKeyDown(KeyCode.Alpha1)) Activate(0, duration);
            if (Input.GetKeyDown(KeyCode.Alpha2)) Activate(1, duration);
            if (Input.GetKeyDown(KeyCode.Alpha3)) Activate(2, duration);
            if (Input.GetKeyDown(KeyCode.Alpha4)) Activate(3, duration);
            if (Input.GetKeyDown(KeyCode.Alpha5)) Activate(4, duration);
            if (Input.GetKeyDown(KeyCode.Alpha6)) Activate(5, duration);
            if (Input.GetKeyDown(KeyCode.Alpha7)) Activate(6, duration);

            if (Input.GetKeyDown(KeyCode.F1)) Deactivate(0, duration);
            if (Input.GetKeyDown(KeyCode.F2)) Deactivate(1, duration);
            if (Input.GetKeyDown(KeyCode.F3)) Deactivate(2, duration);
            if (Input.GetKeyDown(KeyCode.F4)) Deactivate(3, duration);
            if (Input.GetKeyDown(KeyCode.F5)) Deactivate(4, duration);
            if (Input.GetKeyDown(KeyCode.F6)) Deactivate(5, duration);
            if (Input.GetKeyDown(KeyCode.F7)) Deactivate(6, duration);
        }

        void Activate(int slotIndex, float duration)
        {
            if (slotIndex < 0 || slotIndex > 6) return;

            pedestal.Activate(slotIndex, duration);

            if (slotIndex > 5) return;
            var character = characters[slotIndex];
            if (character != null) character.Activate(duration);
        }

        void Deactivate(int slotIndex, float duration)
        {
            if (slotIndex < 0 || slotIndex > 6) return;
            
            pedestal.Deactivate(slotIndex, duration);

            if (slotIndex > 5) return;
            var character = characters[slotIndex];
            if (character != null) character.Deactivate(duration);
        }
    }
}