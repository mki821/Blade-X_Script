using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "EnableNavMesh", story: "Enable [NavMesh] [Enable]", category: "Action", id: "df1e593f7d0ac662e549c75d536c5e2a")]
public partial class EnableNavMeshAction : Action
{
    [SerializeReference] public BlackboardVariable<NavMeshAgent> NavMesh;
    [SerializeReference] public BlackboardVariable<bool> Enable;

    protected override Status OnStart()
    {
        NavMesh.Value.Warp(NavMesh.Value.transform.position);
        NavMesh.Value.enabled = Enable;
        
        return Status.Success;
    }
}

