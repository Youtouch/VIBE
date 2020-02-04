using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class MiniGame : SerializedMonoBehaviour
{
    #region Variables

    [SerializeField, FoldoutGroup("Data")] private MiniGameData m_Data;
    public MiniGameData data => m_Data;

    [SerializeField, ShowIf("isKingOfTheHill"), FoldoutGroup("Data")]
    private GameObject[] m_ObjectiveObjects = null;

    [SerializeField, ShowIf("isCubePlacement"), FoldoutGroup("Data")]
    private CubePattern[] m_AvailablePatterns = null;

    [SerializeField, FoldoutGroup("In Game"), ReadOnly]
    private CubePattern m_CurrentPattern = null;

    [SerializeField, ShowIf("isCubePlacement"), FoldoutGroup("Scene")]
    private Transform m_LeftPatternSpawn = null, m_RightPatternSpawn = null;

    [SerializeField, FoldoutGroup("In Game"), ReadOnly]
    private CubePattern m_LeftPattern, m_RightPattern;


    [SerializeField, ShowIf("isKingOfTheHill"), FoldoutGroup("Data")]
    private PickupController m_ObjectToHold = null;

    [SerializeField, FoldoutGroup("In Game"), ReadOnly]
    private PlayerController m_HoldingPlayer = null;


    [SerializeField, FoldoutGroup("Scene")]
    private PlayerTeamSpawns[] m_StartingPositions = new PlayerTeamSpawns[2];

    [SerializeField, FoldoutGroup("In Game"), ReadOnly]
    private GameObject m_Level = null;

    [SerializeField, FoldoutGroup("Scene")]
    private GameObject[] m_AvailableLevels = new GameObject[0];

    [SerializeField, FoldoutGroup("In Game"), ReadOnly]
    public MiniGameScore scores;

    [SerializeField, FoldoutGroup("In Game"), ReadOnly]
    public bool hasStarted;

    [SerializeField, FoldoutGroup("In Game"), ReadOnly]
    private float m_CurrentScrollingSpeed, m_CurrentScrollTime, m_ScoreTimer, m_TimeLimitTimer;

    private Vector3 m_BaseWorldPosition;

    #endregion

    public void SetupGame()
    {
        m_BaseWorldPosition = MiniGameController.ground.transform.position;
        gameObject.SetActive(true);
        if (m_ObjectToHold)
            m_ObjectToHold.gameObject.SetActive(false);
        PlayerManager.instance.RevivePlayers();
        InitializeScores();
        GlobalUIManager.instance.OpenPageFromScreen(GameScreen.Tutorial);
        var tutorialPage = (TutorialPage) GlobalUIManager.instance.currentPage;
        tutorialPage.Initialize(this);
    }

    public void StartMiniGame()
    {
        // Debug.Log("Starting  " + gameObject.name);
        m_CurrentScrollTime = 0;
        m_ScoreTimer = 0;
        m_TimeLimitTimer = 0;
        m_CurrentScrollingSpeed = data.baseLevelScrollSpeed;
        CameraHandler.UpdateScrollingType(data.cameraScrollingType);
        MiniGameController.RepositionPlayer(m_StartingPositions);
        if (m_Level)
            m_Level.SetActive(false);
        m_Level = ChooseLevel();
        if (m_Level)
            m_Level.SetActive(true);
        else
        {
            Debug.LogError("Error loading level for minigame:  " + data.name);
        }

        GlobalUIManager.instance.HideScoreObjective();
        GlobalUIManager.instance.HideTimeLimit();
        if (data.hasTimeLimit)
        {
            GlobalUIManager.instance.ShowTimeLimit();
        }
        else if (data.hasScoreToReach)
        {
            GlobalUIManager.instance.ShowScoreObjective();
            GlobalUIManager.instance.UpdateScoreObjective(data.scoreToReach);
        }

        if (isRunner)
            SetupRunner();
        if (isKingOfTheHill)
            SetupKotH();
        if (isCubePlacement)
            SetupCubePlacement();
        if (data.shouldRemoveGround)
            MiniGameController.instance.RemoveGroundAfter(data.timeBeforeRemoveGround);
        StartCoroutine(StartWithDelay());
    }

    private void SetupRunner()
    {
        m_Level.transform.position = Vector3.zero;
        m_CurrentScrollTime = 0;
        m_CurrentScrollingSpeed = 0;
    }

    private void SetupKotH()
    {
        m_ObjectToHold.gameObject.SetActive(true);
    }

    private void SetupCubePlacement()
    {
//        Debug.Log("Pattern");
        m_CurrentPattern = ChoosePattern();
        if (m_LeftPattern)
            Destroy(m_LeftPattern);
        if (m_RightPattern)
            Destroy(m_RightPattern);
        m_LeftPattern = Instantiate(m_CurrentPattern, m_LeftPatternSpawn);
        m_RightPattern = Instantiate(m_CurrentPattern, m_RightPatternSpawn);
        m_LeftPattern.team = 0;
        m_RightPattern.team = 1;
    }

    [Button]
    private void EndGame()
    {
        hasStarted = false;
        GlobalUIManager.instance.HideScoreObjective();
        GlobalUIManager.instance.HideTimeLimit();
        HandleLevelCleaning();
        HandleTimeDeath();
        MiniGameController.instance.EndMiniGame(this);
        PlayerManager.instance.RevivePlayers();
    }

    private void HandleLevelCleaning()
    {
        MiniGameController.ground.transform.position = m_BaseWorldPosition;
        if (m_Level)
            m_Level.SetActive(false);
        if (m_HoldingPlayer && m_HoldingPlayer.IsHoldingThisObject(m_ObjectToHold))
            m_HoldingPlayer.Drop();
        if (m_ObjectToHold)
            m_ObjectToHold.gameObject.SetActive(false);
        var spawners = m_Level.GetComponentsInChildren<CubeSpawner>();
        GlobalUIManager.instance.HideScoreObjective();
        GlobalUIManager.instance.HideTimeLimit();
        if (spawners != null)
            foreach (var spawner in spawners)
            {
                if (spawner.transform.childCount <= 0) continue;
                for (var i = 0; i < spawner.transform.childCount; i++)
                {
                    Destroy(spawner.transform.GetChild(0).gameObject);
                }
            }

        if (isCubePlacement)
        {
            m_LeftPattern.Reset();
            m_RightPattern.Reset();
        }
    }

    private void InitializeScores()
    {
        scores = new MiniGameScore();
        scores.gameScores = new PlayerScore[4];
        for (var index = 0; index < scores.gameScores.Length; index++)
        {
            scores.gameScores[index].playerId = index;
        }

        scores.miniGame = data;
    }

    private GameObject ChooseLevel()
    {
        if (m_AvailableLevels.Length == 0)
            return null;
        if (m_AvailableLevels.Length == 1)
            return m_AvailableLevels[0];
        var random = Random.Range(0, m_AvailableLevels.Length);
        if (random == m_AvailableLevels.Length)
            random--;
        return m_AvailableLevels[random];
    }

    private CubePattern ChoosePattern()
    {
        if (m_AvailablePatterns.Length == 0)
            return null;
        if (m_AvailablePatterns.Length == 1)
            return m_AvailablePatterns[0];
        var random = Random.Range(0, m_AvailablePatterns.Length);
        if (random == m_AvailablePatterns.Length)
            random--;
        return m_AvailablePatterns[random];
    }

    private void Update()
    {
        if (!hasStarted)
            return;
        if (isRunner && data.levelScrollingType != LevelScrollingType.None)
        {
            HandleLevelScroll();
        }

        if (data.hasScoreToReach)
        {
            HandlePoints();
        }

        if (data.hasTimeLimit)
        {
            HandleTimeLimit();
        }
    }

    private void HandleLevelScroll()
    {
        if (m_CurrentScrollTime >= data.timeBeforeStopScroll)
            return;
        var multiplier = (data.levelScrollingType == LevelScrollingType.HorizontalLeft || data.levelScrollingType == LevelScrollingType.VerticalDown) ? -1 : 1;
        var vector = (data.levelScrollingType == LevelScrollingType.HorizontalLeft || data.levelScrollingType == LevelScrollingType.HorizontalRight) ? Vector3.right : Vector3.up;
        m_CurrentScrollingSpeed += (data.maxLevelScrollSpeed / data.timeToReachMaxSpeed) * Time.deltaTime;
        var maxSpeed = m_CurrentScrollingSpeed * Time.deltaTime * multiplier * vector;
        m_Level.transform.position += maxSpeed;
        if (data.m_WorldScrollingType == WorldScrollingType.FollowLevel)
        {
            MiniGameController.ground.transform.position += maxSpeed;
        }

        if (data.levelScrollingType == LevelScrollingType.HorizontalLeft || data.levelScrollingType == LevelScrollingType.HorizontalRight)
        {
            for (int i = 0; i < PlayerManager.players.Count; i++)
            {
                PlayerManager.players[i].ForceMove(maxSpeed);
            }
        }

        m_CurrentScrollTime += Time.deltaTime;
    }

    #region Handle Calls

    public void OnPatternMessageReceived(CubePattern pattern)
    {
        if (pattern.patternId != m_CurrentPattern.patternId) return;
        Debug.Log("Winners are team " + pattern.team);
        EndGame();
    }


    public void HandleCallPlayerDied()
    {
        switch (m_Data.type)
        {
            case MiniGameType.Runner:
                var playersAlive = PlayerManager.players.Count(x => x.gameObject.activeSelf);
//                Debug.Log("Players Alive " + playersAlive);
                if (playersAlive <= 0)
                    EndGame();
                break;
            case MiniGameType.KingOfTheHill:
                PlayerManager.instance.RevivePlayers();
                break;
            case MiniGameType.CubePlacement:
                PlayerManager.instance.RevivePlayers();
                break;
        }
    }

    public void HandleCallObjectiveReached(PlayerController player)
    {
        for (var index = 0; index < scores.gameScores.Length; index++)
        {
            if (index != player.idInManager)
                scores.gameScores[index].score = 0;
        }

        EndGame();
    }

    public void HandleCallObjectGrabbed(PickupController grabbedObject, PlayerController holder)
    {
        if (grabbedObject == m_ObjectToHold)
        {
            m_HoldingPlayer = holder;
            m_ScoreTimer = 0;
        }
    }

    public void HandleCallObjectStolen(PickupController grabbedObject, PlayerController newHolder, PlayerController exHolder = null)
    {
        if (grabbedObject == m_ObjectToHold)
        {
        }
    }

    public void HandleCallObjectDroped(PickupController grabbedObject, PlayerController exHolder)
    {
        if (grabbedObject == m_ObjectToHold)
        {
            m_HoldingPlayer = null;
            m_ScoreTimer = 0;
        }
    }

    #endregion

    private void HandlePoints()
    {
        if (m_HoldingPlayer != null)
        {
            m_ScoreTimer += Time.deltaTime;
            if (m_ScoreTimer >= 1)
            {
                m_ScoreTimer = 0;
                scores.gameScores[m_HoldingPlayer.idInManager].score += data.pointsPerSecond;
                CheckPointsVictory();
            }
        }
    }

    public void GivePoints(int pointsToGive, int playerId)
    {
        scores.gameScores[playerId].score += pointsToGive;
        CheckPointsVictory();
    }

    private void CheckPointsVictory()
    {
        if (!data.hasScoreToReach)
            return;
        var tempScores = scores.gameScores;
        var highestScore = tempScores.OrderByDescending(x => x.score).ToArray()[0];
        if (highestScore.score >= data.scoreToReach)
            EndGame();
    }

    private void HandleTimeLimit()
    {
//        Debug.Log("time caca");
        m_TimeLimitTimer += Time.deltaTime;
        HandleTimeScore();
        GlobalUIManager.instance.UpdateTimer((int) m_TimeLimitTimer, data.timeLimit);
        if (m_TimeLimitTimer >= data.timeLimit)
        {
            m_TimeLimitTimer = 0;
            EndGame();
        }
    }

    private void HandleTimeScore()
    {
        if (data.hasScoreToReach || isCubePlacement) return;
        for (var index = 0; index < PlayerManager.players.Count; index++)
        {
            var player = PlayerManager.players[index];
            if (player.gameObject.activeSelf)
            {
                scores.gameScores[index].score = (int) m_TimeLimitTimer;
            }
        }
    }

    private void HandleTimeDeath()
    {
        if (!data.hasTimeLimit) return;
        for (var index = 0; index < PlayerManager.players.Count; index++)
        {
            var player = PlayerManager.players[index];
            if (!player.gameObject.activeSelf)
            {
                scores.gameScores[index].score = 0;
            }
        }
    }

    private IEnumerator StartWithDelay()
    {
        yield return new WaitForSeconds(data.timeBeforeScrolling);

        hasStarted = true;
    }

    #region EditorHelpers

    public bool isRunner => data != null && data.type == MiniGameType.Runner;
    public bool isKingOfTheHill => data != null && data.type == MiniGameType.KingOfTheHill;
    public bool isCubePlacement => data != null && data.type == MiniGameType.CubePlacement;

    #endregion
}

[System.Serializable]
public struct PlayerTeamSpawns
{
    public int team;
    public Transform[] startingPosition;
}