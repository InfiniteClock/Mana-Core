using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Timer", story: "Counts [timer] up towards [timeLimit]", category: "Action", id: "174410eb9965625db7e13f989350de97")]
public partial class TimerAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Timer;
    [SerializeReference] public BlackboardVariable<float> TimeLimit;

    

    protected override Status OnUpdate()
    {
        if (Timer == null || TimeLimit == null) return Status.Failure;

        if (Timer.Value < TimeLimit.Value)
            Timer.Value += Time.deltaTime;

        return Status.Success;
    }

}

