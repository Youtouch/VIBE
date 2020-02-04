using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreCard : MonoBehaviour
{
    [SerializeField] private Image m_PlayerImage;
    [SerializeField] private TextMeshProUGUI m_Score, m_PlayerNumber;
    [SerializeField] private Transform m_ItemsParent;

    public void Initialize(PlayerController player)
    {
        m_PlayerNumber.gameObject.SetActive(false);
        m_PlayerImage.sprite = GlobalUIManager.instance.characterImages[player.idInManager].characterImage;
        if (m_PlayerImage.sprite != null) return;
        m_PlayerNumber.gameObject.SetActive(true);
        m_PlayerNumber.text = player.idInManager.ToString();
    }

    public void UpdateScore(string newScoreText)
    {
        m_Score.text = newScoreText;
    }

    public void AddItem(ItemData item)
    {
        var img = Instantiate(m_PlayerImage, m_ItemsParent);
        img.sprite = item.icon;
    }
}