using UnityEngine;

public class WeaponAnimatorContainer : MonoBehaviour
{
    private Animator[] weaponAnimators;

    void Awake()
    {
        weaponAnimators = GetComponentsInChildren<Animator>();
    }

    public Animator[] GetAnimators()
    {
        return weaponAnimators;
    }
}
