using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class MiniGameData : SerializedScriptableObject
{
    [SerializeField, FoldoutGroup("Rules")]
    public MiniGameType type = MiniGameType.Runner;

    [SerializeField, FoldoutGroup("Rules")]
    public ItemData reward;

    [SerializeField, FoldoutGroup("Scrolling")]
    public LevelScrollingType levelScrollingType = LevelScrollingType.None;

    [SerializeField, FoldoutGroup("Scrolling")]
    public CameraScrollingType cameraScrollingType = CameraScrollingType.FollowPlayers;

    [SerializeField, FoldoutGroup("UI")] public Sprite image;
    [SerializeField, FoldoutGroup("UI")] public string description, title;

    [SerializeField, FoldoutGroup("Rules")]
    public bool hasTimeLimit = false;

    [ShowIf("hasTimeLimit"), FoldoutGroup("Rules")]
    public int timeLimit;

    [SerializeField, FoldoutGroup("Rules")]
    public bool hasScoreToReach = false;

    [ShowIf("hasScoreToReach"), FoldoutGroup("Rules")]
    public int scoreToReach, pointsPerSecond;

    [ShowIf("hasScrolling"), SerializeField, FoldoutGroup("Scrolling")]
    public float timeBeforeScrolling, baseLevelScrollSpeed, maxLevelScrollSpeed, timeToReachMaxSpeed = 1;

    [ShowIf("hasScrolling"), SerializeField, FoldoutGroup("Scrolling")]
    public bool stopsScrollingAfterTime = true;

    [ShowIf("hasScrolling"), ShowIf("stopsScrollingAfterTime"), FoldoutGroup("Scrolling")]
    public float timeBeforeStopScroll;


    [SerializeField, FoldoutGroup("Gameplay")]
    public WorldScrollingType m_WorldScrollingType;
    public bool shouldRemoveGround => m_WorldScrollingType == WorldScrollingType.Deactivate;

    [ShowIf("shouldRemoveGround"), FoldoutGroup("Gameplay")]
    public float timeBeforeRemoveGround;

    public bool hasScrolling => levelScrollingType != LevelScrollingType.None;
}

public enum MiniGameType
{
    Runner,
    KingOfTheHill,
    CubePlacement
}

public enum LevelScrollingType
{
    HorizontalLeft,
    HorizontalRight,
    VerticalUp,
    VerticalDown,
    None,
}
public enum WorldScrollingType
{
    Deactivate,
    FollowLevel,
    None
}
public enum CameraScrollingType
{
    None,
    FollowPlayers,
}