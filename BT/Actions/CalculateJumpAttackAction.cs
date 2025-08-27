using System;
using Swift_Blade.Enemy.Sword;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CalculateJumpAttack", story: "Calculate Jump Attack Position", category: "Action", id: "144231b3ae614aaf4db3af2f2b299f99")]
public partial class CalculateJumpAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<SwordBossAnimatorController> animatorController;
        
    protected override Status OnStart()
    {
        animatorController.Value.CalculateJumpAttackPosition();
        return Status.Success;
    }

}

