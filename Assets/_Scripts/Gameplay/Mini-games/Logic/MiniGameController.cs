using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class MiniGameController : SingletonBehaviour<MiniGameController>
{
    [SerializeField, FoldoutGroup("GameData")]
    private List<MiniGame> m_AvailableGames = new List<MiniGame>();

    [SerializeField, FoldoutGroup("Scene")]
    private GameObject m_Ground = null;
    public static GameObject ground => instance.m_Ground;

    [SerializeField, FoldoutGroup("Gameplay"), ReadOnly]
    private List<MiniGame> m_PastGames = new List<MiniGame>();

    [SerializeField, FoldoutGroup("Gameplay"), ReadOnly]
    private MiniGame m_CurrentMiniGame = null;

    public static MiniGame current => instance.m_CurrentMiniGame;


    [SerializeField, FoldoutGroup("Gameplay"), ReadOnly]
    private List<MiniGameScore> m_PastScores = new List<MiniGameScore>();

    [SerializeField, FoldoutGroup("Gameplay"), ReadOnly]
    private int m_CurrentMiniGameId = -1;

    [SerializeField, FoldoutGroup("Gameplay"), ReadOnly]
    private bool m_InPracticeMode = false;

    [SerializeField, FoldoutGroup("Scene")]
    private PlayerTeamSpawns[] m_BaseStartingPositions = new PlayerTeamSpawns[2];

    [SerializeField, FoldoutGroup("Scene")]
    public MiniGame m_ConstructionFinalGame;

    public void StartMiniGame(bool isPractice)
    {
//        Debug.Log("Starting Minigame: "+ m_CurrentMiniGame.data.title);
        m_InPracticeMode = isPractice;
        m_CurrentMiniGame.StartMiniGame();
    }

    public int currentMiniGameId => m_CurrentMiniGameId;

    public void SetCurrentMiniGameId(int id)
    {
        m_CurrentMiniGameId = id;
    }

    public void SetScoresFromData(List<MiniGameScore> scoresToSet)
    {
        m_PastScores = scoresToSet;
    }

    public static void StartGame(MiniGame gameToStart)
    {
        instance.m_PastGames.Add(gameToStart);
        instance.m_CurrentMiniGame = gameToStart;
        instance.m_CurrentMiniGame.SetupGame();
    }


    public void StartNextMiniGame()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("This function is not available in editor <3");
            return;
        }

        if (m_CurrentMiniGame != null)
        {
            Debug.Log("Minigame is still going");
            return;
        }

        PlayerManager.instance.RevivePlayers();
        if (m_CurrentMiniGame)
            Debug.Log("CurrentMinigame" + m_CurrentMiniGame.name);
        m_CurrentMiniGame = GetMiniGame();
        // Debug.Log(m_CurrentMiniGame.name);

        if (m_CurrentMiniGame == null)
        {
            Debug.Log("Minigame is null");
            return;
        }

        m_InPracticeMode = false;
        m_PastGames.Add(m_CurrentMiniGame);
        m_CurrentMiniGame.SetupGame();
    }

    public void EndMiniGame(MiniGame finishedGame)
    {
        if (finishedGame != m_CurrentMiniGame)
        {
            if (finishedGame == null)
            {
                Debug.LogWarning("Finished Game is null");
                return;
            }

            Debug.LogWarning("Finished Minigame different than current one has sent end event:  " + finishedGame.name);
            return;
        }

        ActivateGround();

        if (m_InPracticeMode)
        {
            m_CurrentMiniGame.SetupGame();
            return;
        }

        ReceiveScores(finishedGame.scores);
        finishedGame.gameObject.SetActive(false);
        HandleGameWinner(m_CurrentMiniGame);
        m_CurrentMiniGame = null;
        StartNextMiniGame();
    }

    private void HandleGameWinner(MiniGame game)
    {
        StartCoroutine(WaitForEndGame(game));
    }

    private IEnumerator WaitForEndGame(MiniGame game)
    {
        
        MiniGameScore scoreToCheck = game.scores;
        var highestScore = 0;
        var winner = -1;
        for (var index = 0; index < scoreToCheck.gameScores.Length; index++)
        {
            var score = scoreToCheck.gameScores[index];
            if (score.score > highestScore)
            {
                highestScore = score.score;
                winner = index;
            }
        }

        if (game.data.reward && winner != -1)
        {
            PlayerManager.players[winner].items.Add(game.data.reward);
            GlobalUIManager.instance.GetScoreCard(winner).AddItem(game.data.reward);
        }

        if (m_CurrentMiniGame == m_ConstructionFinalGame)
        {
            yield return new WaitForSeconds( 1.5f);
            GlobalUIManager.instance.TriggerEndOfGame(PlayerManager.players[winner]);
        }
    }

    private void ReceiveScores(MiniGameScore score)
    {
        m_PastScores.Add(score);
    }

    private MiniGame GetMiniGame()
    {
        switch (GameManager.gameMode)
        {
            case GameMode.RandomGames:
            {
                var remainingGames = m_AvailableGames.Where(x => !m_PastGames.Contains(x)).ToArray();
                if (remainingGames.Length == 0)
                {
                    m_PastGames.Clear();
                    var randClear = Random.Range(0, m_AvailableGames.Count);
                    return m_AvailableGames[randClear];
                }

                var randGame = Random.Range(0, remainingGames.Length);
                return remainingGames[randGame];
            }
            case GameMode.StoryMode:
            {
                m_CurrentMiniGameId++;
                return m_CurrentMiniGameId >= m_AvailableGames.Count ? m_ConstructionFinalGame : m_AvailableGames[m_CurrentMiniGameId];
            }
            default:
                return null;
        }
    }

    public void RemoveGroundAfter(float time)
    {
        StartCoroutine(RemoveGround(time));
    }

    private IEnumerator RemoveGround(float time)
    {
        yield return new WaitForSeconds(time);
        m_Ground.SetActive(false);
    }

    private void ActivateGround()
    {
        m_Ground.gameObject.SetActive(true);
        RepositionPlayer(m_BaseStartingPositions);
    }

    public static void RepositionPlayer(PlayerTeamSpawns[] m_StartingPositions)
    {
        var testedLocations_team1 = new List<Transform>();
        var testedLocations_team2 = new List<Transform>();

        foreach (var player in PlayerManager.players)
        {
            if (player.team == 0)
            {
                var team1Pool = m_StartingPositions.First(x => x.team == 0);
                var availableLocations = team1Pool.startingPosition.Where(x => !testedLocations_team1.Contains(x)).ToList();
                var randomTeam1SpotId = Random.Range(0, availableLocations.Count);
                var foundSpot = team1Pool.startingPosition[randomTeam1SpotId];
                testedLocations_team1.Add(foundSpot);
                player.transform.position = foundSpot.position;
                continue;
            }

            var team2Pool = m_StartingPositions.First(x => x.team == 1);
            var availableLocations2 = team2Pool.startingPosition.Where(x => !testedLocations_team2.Contains(x)).ToList();
            var randomTeam2SpotId = Random.Range(0, availableLocations2.Count);
            var foundSpot2 = team2Pool.startingPosition[randomTeam2SpotId];
            testedLocations_team2.Add(foundSpot2);
            player.transform.position = foundSpot2.position;
        }
    }

    public static void RepositionDeadPlayer()
    {
        var m_StartingPositions = instance.m_BaseStartingPositions;
        var testedLocations_team1 = new List<Transform>();
        var testedLocations_team2 = new List<Transform>();

        foreach (var player in PlayerManager.players)
        {
            if (player.gameObject.activeSelf)
                continue;
            if (player.team == 0)
            {
                var team1Pool = m_StartingPositions.First(x => x.team == 0);
                var availableLocations = team1Pool.startingPosition.Where(x => !testedLocations_team1.Contains(x)).ToList();
                var randomTeam1SpotId = Random.Range(0, availableLocations.Count);
                var foundSpot = team1Pool.startingPosition[randomTeam1SpotId];
                testedLocations_team1.Add(foundSpot);
                player.transform.position = foundSpot.position;
                continue;
            }

            var team2Pool = m_StartingPositions.First(x => x.team == 1);
            var availableLocations2 = team2Pool.startingPosition.Where(x => !testedLocations_team2.Contains(x)).ToList();
            var randomTeam2SpotId = Random.Range(0, availableLocations2.Count);
            var foundSpot2 = team2Pool.startingPosition[randomTeam2SpotId];
            testedLocations_team2.Add(foundSpot2);
            player.transform.position = foundSpot2.position;
        }
    }
}