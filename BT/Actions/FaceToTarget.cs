using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Swift_Blade.Enemy;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FaceToTarget", story: "[Self] Face to [Target]", category: "Action", id: "d87da8f155dbf7af16c1227b49324330")]
public partial class FaceToTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<BaseEnemy> Self;
    [SerializeReference] public BlackboardVariable<Transform> Target;

    protected override Status OnStart()
    {
        Vector3 a = Self.Value.transform.position;
        a.y = 0f;
        Vector3 b = Target.Value.position;
        b.y = 0f;

        Self.Value.transform.rotation = Quaternion.LookRotation(b - a);

        return Status.Success;
    }
}

