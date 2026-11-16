using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TargetViewCheck", story: "Updates [TargetView] from [CurrentTarget]", category: "Action", id: "0ff9684995043621906b773ecf90f49e")]
public partial class TargetViewCheckAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> TargetView;
    [SerializeReference] public BlackboardVariable<GameObject> CurrentTarget;
    private Transform mainCamTransform;

    protected override Status OnUpdate()
    {
        if (CurrentTarget.Value == null) return Status.Failure;

        if (CurrentTarget.Value.TryGetComponent(out PlayerMovement pMove))
        {
            if (mainCamTransform == null)
                mainCamTransform = pMove.playerCam.transform;

            TargetView.Value = mainCamTransform;
        }
        else
        {
            TargetView.Value = null;
        }

        return Status.Success;
    }

}

