using System;
using _KIT.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

public class Bow : BaseWeapon
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform parent;
    [SerializeField] private GameObject arrowPrefab;

    private GameObject instance;
    
    public override void RequestAttack()
    {
        animator.Play("Attack");
    }

    public override void Trigger()
    {
        instance = Instantiate(arrowPrefab);
        instance.transform.position = parent.position;
        instance.transform.rotation = parent.rotation;
        this.WaitInvoke(2, () =>
        {
            Destroy(instance);
            instance = null;
        });
    }

    private void Update()
    {
        if (instance != null)
        {
            instance.transform.position += Vector3.right * 5 * Time.deltaTime; 
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            RequestAttack();
        }
    }
}