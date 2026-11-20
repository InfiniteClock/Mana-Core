using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "TargetViewToSelf", story: "[TargetView] angle to [Self] is [Operator] than [float]", category: "Conditions", id: "4e1bec102e9fe862d1f997e69b884c25")]
public partial class TargetViewToSelfCondition : Condition
{
    [SerializeReference] public BlackboardVariable<Transform> TargetView;
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [Comparison(comparisonType: ComparisonType.All)]
    [SerializeReference] public BlackboardVariable<ConditionOperator> Operator;
    [SerializeReference] public BlackboardVariable<float> Float;

    public override bool IsTrue()
    {
        if (Self.Value == null || TargetView.Value == null) return false;

        // Compares the angle of the target's forward view to the angle between targetview object and self
        Vector3 dirToTarget = (TargetView.Value.position - Self.Value.transform.position).normalized;
        float angle = Vector3.Angle(TargetView.Value.forward, dirToTarget);


        return ConditionUtils.Evaluate(angle, Operator, MathF.Abs(Float.Value));
    }

}
