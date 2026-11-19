using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FlyerEvade", story: "[Self] moves [RollForce] using [FlyerSelf]", category: "Action", id: "725a30c967774f79c146593a572e8bd7")]
public partial class FlyerEvadeAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<float> RollForce;
    [SerializeReference] public BlackboardVariable<FlyerEnemy> FlyerSelf;

    protected override Status OnUpdate()
    {
        int direction = UnityEngine.Random.Range(0, 2) * 2 - 1;
        FlyerSelf.Value.BarrelRoll(direction, RollForce.Value);
        return Status.Success;
    }

}

