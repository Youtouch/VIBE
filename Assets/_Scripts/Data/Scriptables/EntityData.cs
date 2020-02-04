using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class EntityData : SerializedScriptableObject
{
    [SerializeField, FoldoutGroup("Movements")]
    public float baseMovementSpeed =0, maxMovementSpeed =0, minMovementSpeed = 0;
    [SerializeField, FoldoutGroup("Jump")]
    public float baseJumpStrenght = 0, minJumpStrenght = 0, maxJumpStrenght = 0, maxFallSpeed = 0, gravityForce = 0, maxJumpTime = 0.5f, jumpForceOverTime = 0.5f, hoverStrenght = 2;
    [SerializeField, FoldoutGroup("Throw")]
    public float baseMaxThrowStrenght = 0, minThrowStrenght = 0, maxThrowStrenght = 0, throwAngle = 0, grapDistanceModifier = 0.2f;
    [SerializeField, FoldoutGroup("ThrowBuild")]
    public float buildUpMax = 0, buildUpMin = 0, buildUpRate = 0;
    [SerializeField, FoldoutGroup("Hug")]
    public float timeHugAttack = 3, timeHugDefend = 2;

    [SerializeField, FoldoutGroup("VFX")]
    public GameObject throwBuildUpVFX, arrowVFX, heartVFX, airVFX;
}
