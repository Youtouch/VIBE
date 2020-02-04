using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPage : UIPage
{
    [SerializeField] private Image m_GameImage = null;
    [SerializeField] private TextMeshProUGUI m_GameDescription = null, m_GameTitle = null;

    [SerializeField] private MiniGame m_CurrentMiniGame;

    [SerializeField, ReadOnly] private bool m_IsStarting = false;

    public void Initialize(MiniGame gameToLaunch)
    {
        m_CurrentMiniGame = gameToLaunch;
        m_IsStarting = false;
        m_GameImage.sprite = m_CurrentMiniGame.data.image;
        m_GameDescription.text = m_CurrentMiniGame.data.description;
        m_GameTitle.text = m_CurrentMiniGame.data.title;
    }

    public void StartNormalGame()
    {
        if(m_IsStarting)
            return;
        m_IsStarting = true;
        StartCoroutine(StartMinigameDelay(false));
        // MiniGameController.instance.StartMiniGame(false);
        Close();
    }

    public void StartPracticeGame()
    {
        if(m_IsStarting)
            return;
        m_IsStarting = true;
        StartCoroutine(StartMinigameDelay(true));
        // MiniGameController.instance.StartMiniGame(true);
        Close();
    }

    private IEnumerator StartMinigameDelay(bool isPractice)
    {
        yield return new WaitForSeconds(1.1f);
        MiniGameController.instance.StartMiniGame(isPractice);
    }

}