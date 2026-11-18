using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "UpdateWeaponTarget", story: "Update [EnemyWeapon] target from [CurrentTarget]", category: "Action", id: "233a7e4457ff582d9099c3f21b2878d1")]
public partial class UpdateWeaponTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyWeapon> EnemyWeapon;
    [SerializeReference] public BlackboardVariable<GameObject> CurrentTarget;

    
    protected override Status OnUpdate()
    {
        if (EnemyWeapon == null) return Status.Failure;

        if (CurrentTarget.Value != null)
            EnemyWeapon.Value.target = CurrentTarget.Value.transform.position;

        return Status.Success;
    }

}

