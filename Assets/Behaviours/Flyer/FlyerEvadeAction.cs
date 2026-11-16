using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FlyerEvade", story: "[Self] moves [distance] towards [EvadeDestination] using [FlyerSelf]", category: "Action", id: "725a30c967774f79c146593a572e8bd7")]
public partial class FlyerEvadeAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<float> Distance;
    [SerializeReference] public BlackboardVariable<Vector3> EvadeDestination;
    [SerializeReference] public BlackboardVariable<FlyerEnemy> FlyerSelf;

    protected override Status OnUpdate()
    {
        int direction = UnityEngine.Random.Range(0, 2) * 2 - 1;
        EvadeDestination.Value = Self.Value.transform.position + Self.Value.transform.right * direction * Distance.Value;
        FlyerSelf.Value.BarrelRoll(direction);
        return Status.Success;
    }

}

