using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Random Value", story: "Sets [float] to random value", category: "Action", id: "6e7e8ac1586aa2c7df85e0b049b94be0")]
public partial class RandomValueAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Float;

    
    protected override Status OnUpdate()
    {
        Float.Value = Mathf.Round(UnityEngine.Random.value * 100f) / 100f;
        return Status.Success;
    }

}

