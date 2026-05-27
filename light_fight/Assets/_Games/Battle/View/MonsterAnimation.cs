using System;
using _KITSystem.Resource;
using Animancer;
using UnityEngine;

public class MonsterAnimation : MonoBehaviour
{
    [SerializeField] private Transform root;
    [SerializeField] private Transform flip;
    [SerializeField] private AnimancerComponent animancer;
    [SerializeField] private AnimationClip idleClip;
    [SerializeField] private AnimationClip moveClip;
    [SerializeField] private AnimationClip attackClip;

    private Vector3 localScale;
    private bool defaultFace = false; // false: left, true: right
    
    private void Awake()
    {
        localScale = flip.localScale;
    }

    public void SetPosition(Vector3 pos)
    {
        root.position = pos;

        bool right = pos.x < 0;
        if (right != defaultFace)
        {
            defaultFace = right;
            flip.localScale = new Vector3(localScale.x * (right ? -1 : 1), localScale.y, localScale.z);
        }
    }

    public void SetActive(bool active)
    {
        root.gameObject.SetActive(active);
    }

    public void Dead()
    {
        SetActive(false);
        KitPool.Destroy(gameObject);
    }

    public void PlayIdle() => animancer.Play(idleClip);
    public void PlayMove() => animancer.Play(moveClip);
}
