using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_PlayerName, m_WinnerText, m_AchievementTemplate;
    [SerializeField] private Image m_PlayerImage;
    [SerializeField] private Transform m_AchievementsParent;


    public void Initialize(PlayerController player)
    {
        m_PlayerName.text = "Player " + (player.idInManager +1);
        m_PlayerImage.sprite = GlobalUIManager.instance.characterImages[player.idInManager].characterImage;
        
    }

    public void SetWinnerState(bool isWinner)
    {
        m_WinnerText.text = isWinner ? "WINNER" : "We still love you";
    }

    public void AddAchievementCard(string achievement)
    {
        var newAchiev = Instantiate(m_AchievementTemplate, m_AchievementsParent);
        newAchiev.text = achievement;
    }
}
