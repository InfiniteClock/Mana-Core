using Unity.Behavior;
using UnityEngine;

[RequireComponent (typeof(FodderFlame))]
[RequireComponent (typeof(FodderMelee))]
[RequireComponent(typeof(FodderExplode))]
public class FodderEnemy : Enemy
{
    private BehaviorGraphAgent agent;
    private BlackboardVariable<FodderSecondaryState> fSecondState;

    private FodderFlame fFlame;
    private FodderMelee fMelee;
    private FodderExplode fExplode;
    protected override void Start()
    {
        base.Start();
        fFlame = GetComponent<FodderFlame>();
        fMelee = GetComponent<FodderMelee>();
        fExplode = GetComponent<FodderExplode>();
        agent = GetComponent<BehaviorGraphAgent>();

        agent.GetVariable("SecondaryState", out fSecondState);
    }
    protected override void Update()
    {
        if (!hasTriggeredDeath && health.isDead)
        {
            // Ensures that a primed enemy will explode upon death
            if (fSecondState.Value == FodderSecondaryState.Prime)
                fExplode.Detonate();
            Die();
        }
    }
}
