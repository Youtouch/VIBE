using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{

    [SerializeField, FoldoutGroup("Gameplay"), ReadOnly]
    private GamePhase m_Phase;

    public GamePhase phase => m_Phase;


    [SerializeField, FoldoutGroup("Gameplay"), ReadOnly]
    private GameState m_State;

    public GameState state => m_State;


    [SerializeField, FoldoutGroup("Gameplay"), ReadOnly]
    private GameMode m_Mode;

    public static GameMode gameMode => instance.m_Mode;

    public void OnGameStart(GameStateData data)
    {
        for (var i = 1; i < data.participatingPlayers; i++)
        {
            PlayerManager.instance.AddPlayer();
        }

        m_Mode = data.mode;
        MiniGameController.instance.SetCurrentMiniGameId(data.currentMiniGame);
        MiniGameController.instance.SetScoresFromData(data.savedScores);
        MiniGameController.instance.StartNextMiniGame();
    }

    public void OnPauseChanged(bool shouldPause)
    {
        if (shouldPause && state != GameState.Paused)
        {
            Debug.Log("Pause Game");
            m_State = GameState.Paused;
            return;
        }

        if (shouldPause || state != GameState.Paused) return;
        GlobalUIManager.instance.ClosePage();
        Debug.Log("Unpause Game");
        m_State = GameState.Playing;
        return;
    }

}


public enum GamePhase
{
    MiniGame,
    Resolution
}


public enum GameState
{
    Playing,
    Paused
}

public enum GameMode
{
    RandomGames,
    StoryMode,
}