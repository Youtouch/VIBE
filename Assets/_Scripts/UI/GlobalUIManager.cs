using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalUIManager : SingletonBehaviour<GlobalUIManager>
{
    [SerializeField] private List<UIPage> m_AllPages;
    private Stack<UIPage> m_Pages = new Stack<UIPage>();
    private UIPage m_CurrentPage = null;
    public UIPage currentPage => m_CurrentPage;

    [SerializeField] public Dictionary<PlayerTrackerVariable, string> achievementsNames = new Dictionary<PlayerTrackerVariable, string>();

    [SerializeField] private GameStateData[] m_Saves = new GameStateData[3];

    [SerializeField] private int m_SelectedSave = 0;
    public static GameStateData saveSlot => instance.m_Saves[instance.m_SelectedSave];
    [SerializeField] public CharacterImages[] characterImages = new CharacterImages[4];

    [SerializeField] private ObjectPool<PlayerScoreCard> m_ScoreCardsPool;
    [SerializeField] private Transform m_ScoreCardParent;

    [SerializeField] private ObjectPool<EndGameCard> m_EndGameCardsPool;
    [SerializeField] private Transform m_EndGameCardParent, m_TimerParent, m_ScoreParent;
    [SerializeField] private TextMeshProUGUI m_ScoreText, m_TimeText;

    public PlayerScoreCard GetScoreCard(int id)
    {
        return m_ScoreCardsPool.rentedObjects[id];
    }

    public void AddScoreCard(PlayerController who)
    {
        var newCard = m_ScoreCardsPool.Rent();
        newCard.Initialize(who);
        newCard.transform.SetParent(m_ScoreCardParent);
    }

    public void ClosePage()
    {
        if (m_Pages.Count == 0 && GameManager.instance.state != GameState.Playing)
        {
            var mainMenuPage = GetPageFromScreen(GameScreen.MainMenu);
            if (mainMenuPage)
                OpenPageWithDelay(mainMenuPage);
            return;
        }

        if (m_Pages.Count == 0)
        {
            return;
        }

        var pageToClose = m_Pages.Pop();
        pageToClose.Close();
        m_CurrentPage = null;
    }

    public void OpenPage(UIPage pageToOpen)
    {
        m_Pages.Push(pageToOpen);
        pageToOpen.Open();
        m_CurrentPage = pageToOpen;
    }

    public void OpenPageFromScreen(GameScreen screenToOpen)
    {
        var page = GetPageFromScreen(screenToOpen);
        if (page)
            OpenPage(page);
    }

    public void SelectGameMode(GameMode mode)
    {
        saveSlot.mode = mode;
        ClosePage();
        StartCoroutine(StartGameDelay());
    }

    public void SelectSaveSlot(int id)
    {
        m_SelectedSave = id;
        ClosePage();
        var playerSelectionPage = GetPageFromScreen(GameScreen.PlayerSelectionScreen);
        if (playerSelectionPage)
            OpenPageWithDelay(playerSelectionPage);
    }

    public void SelectNumberOfPlayers(int id)
    {
        m_Saves[m_SelectedSave].participatingPlayers = id;
        ClosePage();
        var levelSelectionPage = GetPageFromScreen(GameScreen.LevelSelection);
        if (levelSelectionPage)
            OpenPageWithDelay(levelSelectionPage);
    }

    public void OpenPageWithDelay(UIPage pageToOpen)
    {
        StartCoroutine(OnOpenPageWithDelay(pageToOpen));
    }

    private IEnumerator OnOpenPageWithDelay(UIPage pageToOpen)
    {
        yield return new WaitForSeconds(1.5f);
        OpenPage(pageToOpen);
    }

    private UIPage GetPageFromScreen(GameScreen screenToOpen)
    {
        return m_AllPages.FirstOrDefault(x => x.screen == screenToOpen);
    }

    private IEnumerator StartGameDelay()
    {
        yield return new WaitForSeconds(1.5f);
        GameManager.instance.OnGameStart(saveSlot);
    }

    public void ShowSaves(bool delayed)
    {
        ClosePage();
        var saveSelectionPage = GetPageFromScreen(GameScreen.SaveSelection);
        if (!saveSelectionPage)
            return;
        if (delayed)
        {
            OpenPageWithDelay(saveSelectionPage);
            return;
        }

        OpenPage(saveSelectionPage);
    }

    private void CheckForAnyKeyInMainMenu()
    {
        if (Input.anyKeyDown || Input.GetButtonDown("Submit"))
        {
            PlayerManager.instance.AddPlayer();
            ClosePage();
            ShowSaves(false);
        }
    }

    private void UpdateScores()
    {
        for (var index = 0; index < m_ScoreCardsPool.rentedObjects.Length; index++)
        {
            var scoreCard = m_ScoreCardsPool.rentedObjects[index];
            scoreCard.UpdateScore(MiniGameController.current.scores.gameScores[index].score.ToString());
        }
    }

    public void TriggerEndOfGame(PlayerController winner)
    {
        Debug.Log("WHOOOHOO LA TEAM " + winner.team + " A GAGNE");

        foreach (var player in PlayerManager.players)
        {
            CreateEndGameCard(player, player.team == winner.team);
        }

        for (int i = 0; i < 6; i++)
        {
            var trackedValue = (PlayerTrackerVariable) i;
            var bestScore = -1;
            var winningPlayerId = 0;
            for (var index = 0; index < PlayerManager.instance.m_PlayerTrackers.Length; index++)
            {
                var tracker = PlayerManager.instance.m_PlayerTrackers[index];
                switch (trackedValue)
                {
                    case PlayerTrackerVariable.hugs:
                        if (tracker.hugs > bestScore)
                        {
                            bestScore = tracker.hugs;
                            winningPlayerId = index;
                        }

                        break;
                    case PlayerTrackerVariable.build:
                        if (tracker.builder > bestScore)
                        {
                            bestScore = tracker.builder;
                            winningPlayerId = index;
                        }

                        break;
                    case PlayerTrackerVariable.rogue:
                        if (tracker.rogue > bestScore)
                        {
                            bestScore = tracker.rogue;
                            winningPlayerId = index;
                        }

                        break;
                    case PlayerTrackerVariable.jump:
                        if (tracker.jumper > bestScore)
                        {
                            bestScore = tracker.jumper;
                            winningPlayerId = index;
                        }

                        break;
                    case PlayerTrackerVariable.win:
                        if (tracker.winner > bestScore)
                        {
                            bestScore = tracker.winner;
                            winningPlayerId = index;
                        }

                        break;
                    case PlayerTrackerVariable.thrower:
                        if (tracker.thrower > bestScore)
                        {
                            bestScore = tracker.thrower;
                            winningPlayerId = index;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            m_EndGameCardsPool.rentedObjects[winningPlayerId].AddAchievementCard(achievementsNames[trackedValue]);
        }

        OpenPageFromScreen(GameScreen.MiniGameResultScreen);
    }

    [Button]
    public void DebugWinners()
    {
        for (int i = 0; i < 4; i++)
        {
            PlayerManager.instance.AddPlayer();
        }

        TriggerEndOfGame(PlayerManager.players[0]);
    }

    private void CreateEndGameCard(PlayerController player, bool isWinner)
    {
        var card = m_EndGameCardsPool.Rent();
        card.transform.SetParent(m_EndGameCardParent);
        card.Initialize(player);
        card.SetWinnerState(isWinner);
    }

    public void AddBuff(BuffData data)
    {
    }

    public void RemoveBuff(BuffData data)
    {
    }

    #region Engine Overrides

    private void Update()
    {
        if (m_CurrentPage && m_CurrentPage.screen == GameScreen.MainMenu)
        {
            CheckForAnyKeyInMainMenu();
        }

        if (MiniGameController.current != null && MiniGameController.current.hasStarted)
        {
            UpdateScores();
        }
    }

    private void Start()
    {
        var mainMenuPage = GetPageFromScreen(GameScreen.MainMenu);
        if (mainMenuPage)
            OpenPage(mainMenuPage);
    }

    #endregion

    public void RestartGame()
    {
        SceneManager.LoadScene(sceneBuildIndex: 0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowTimeLimit()
    {
        m_TimerParent.gameObject.SetActive(true);
        m_TimeText.text = "";
    }

    public void HideTimeLimit()
    {
        m_TimerParent.gameObject.SetActive(false);
        m_TimeText.text = "";
    }

    public void UpdateTimer(int time, int maxTime)
    {
//        Debug.Log("time " + time);
        var realTime = maxTime - time;
        m_TimeText.text = realTime.ToString();
    }

    public void UpdateScoreObjective(int score)
    {
        m_ScoreText.text = score.ToString();
    }

    public void ShowScoreObjective()
    {
        m_ScoreParent.gameObject.SetActive(true);
        m_ScoreText.text = "";
    }

    public void HideScoreObjective()
    {
        m_ScoreParent.gameObject.SetActive(false);
        m_ScoreText.text = "";
    }
}


[System.Serializable]
public enum GameScreen
{
    None,
    MainMenu,
    LevelSelection,
    SaveSelection,
    PauseMenu,
    Tutorial,
    MiniGameResultScreen,
    PlayerSelectionScreen
}

[System.Serializable]
public struct CharacterImages
{
    public Sprite characterImage;
}