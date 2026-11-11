using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "UseWeapon", story: "Triggers [EnemyWeapon] Attack as [isShooting]", category: "Action", id: "11c66173b7fe443577c5dbaf9a3c6dbc")]
public partial class UseWeaponAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyWeapon> EnemyWeapon;
    [SerializeReference] public BlackboardVariable<bool> IsShooting;
    
    protected override Status OnUpdate()
    {
        if (IsShooting.Value)EnemyWeapon.Value.PrimaryFire();
        else EnemyWeapon.Value.PrimaryEnd();
        return Status.Success;
    }

}

