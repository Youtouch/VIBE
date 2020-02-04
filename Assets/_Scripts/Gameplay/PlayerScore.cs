using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerScore
{
    public int playerId;
    public int score;
}

public struct MiniGameScore
{
    public PlayerScore[] gameScores;
    public MiniGameData miniGame;
}