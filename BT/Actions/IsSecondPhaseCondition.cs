using System;
using Swift_Blade.Combat.Health;
using Swift_Blade.Enemy;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsSecondPhase", story: "[BossHealth] Is Second Phase", category: "Conditions", id: "e4a2f4f3eb376f051df169c8cb214741")]
public partial class IsSecondPhaseCondition : Condition
{
    [SerializeReference] public BlackboardVariable<BossHealth> BossHealth;
    public override bool IsTrue()
    {
        return BossHealth.Value.IsSecondPhase;
    }
    
}
