using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameStateData : SerializedScriptableObject
{
    public List<MiniGameScore> savedScores = new List<MiniGameScore>();
    public int currentMiniGame = -1;
    public GameMode mode = GameMode.StoryMode;
    public int participatingPlayers = 2;
}
