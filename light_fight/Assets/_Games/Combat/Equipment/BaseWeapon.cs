using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    public abstract void RequestAttack();
    public abstract void Trigger();
}