using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ResetAnimatorTransform", story: "Reset [Animator] Transform", category: "Action", id: "5a905944f658b05e7df7d1da49eb0d22")]
public partial class ResetAnimatorTransformAction : Action
{
    [SerializeReference] public BlackboardVariable<Animator> Animator;

    protected override Status OnStart()
    {
        Animator.Value.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));

        return Status.Success;
    }
}

