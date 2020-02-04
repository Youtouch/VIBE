using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelChoiceObject : MonoBehaviour
{
    [SerializeField] private ParticleSystem m_DisplayText;
    [SerializeField] private LevelChoiceType m_ChoiceType;
    [SerializeField, ShowIf("isMode")] private GameMode m_Mode;
    [SerializeField, ShowIf("isSave")] private int m_StateId;
    [SerializeField, ShowIf("isPlayers")] private int m_PlayerCount;

    [SerializeField, ShowIf("isMiniGameMode")]
    private bool m_IsPractice;

    private bool m_Chose;
    [SerializeField] private ParticleSystem m_SelectionParticles;
    private bool isActive = false;
    [SerializeField] private TMP_FontAsset m_NormalFont, m_SelectedFont;
    [SerializeField] private TextMeshPro m_DescriptionText;

    private void OnEnable()
    {
        isActive = false;
        m_Chose = false;
        StartCoroutine(DelayedActivation());
    }

    private void DisplaySelectionText()
    {
        if (!m_DisplayText.isPlaying)
            m_DisplayText.Play();
        if (m_DescriptionText.font != m_SelectedFont)
        {
            m_DescriptionText.font = m_SelectedFont;
            m_DescriptionText.Rebuild(CanvasUpdate.PostLayout);
        }
    }

    private void HideSelectionText()
    {
        if (m_DisplayText.isPlaying)
            m_DisplayText.Stop();
        if (m_DescriptionText.font != m_NormalFont)
        {
            m_DescriptionText.font = m_NormalFont;
            m_DescriptionText.Rebuild(CanvasUpdate.PostLayout);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isActive || m_Chose || !other.gameObject.CompareTag("Player"))
            return;
        DisplaySelectionText();

        var player = other.gameObject.GetComponent<PlayerController>();

        if ((Input.GetButton("Jump_P1") && player.idInManager == 0) || (Input.GetButton("Jump_P2") && player.idInManager == 1) ||
            (Input.GetButton("Jump_P3") && player.idInManager == 2) || (Input.GetButton("Jump_P4") && player.idInManager == 3))
        {
            m_Chose = true;
            HandleChoice(player);
            return;

            return;
        }

        if (m_ChoiceType == LevelChoiceType.TriggerObjective)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                m_Chose = true;
                HandleChoice(player);
                return;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isActive)
            return;
        HideSelectionText();
    }

    private void PlaySelectionVFX()
    {
        if (m_SelectionParticles)
            m_SelectionParticles.Play();
    }

    private void HandleChoice(PlayerController player)
    {
        isActive = false;
        GameAudioManager.instance.PlaySoundOneShot(AudioEventType.ValidateMenu, transform.position);
        HideSelectionText();
        PlaySelectionVFX();
        if (m_ChoiceType == LevelChoiceType.Mode)
        {
            GlobalUIManager.instance.SelectGameMode(m_Mode);
            return;
        }

        if (m_ChoiceType == LevelChoiceType.RestartGame)
        {
            GlobalUIManager.instance.RestartGame();
            return;
        }

        if (m_ChoiceType == LevelChoiceType.QuitGame)
        {
            GlobalUIManager.instance.QuitGame();
            return;
        }

        if (m_ChoiceType == LevelChoiceType.NumberOfPlayers)
        {
            GlobalUIManager.instance.SelectNumberOfPlayers(m_PlayerCount);
            return;
        }

        if (m_ChoiceType == LevelChoiceType.Save)
        {
            GlobalUIManager.instance.SelectSaveSlot(m_StateId);
            return;
        }

        if (m_ChoiceType == LevelChoiceType.MiniGameMode)
        {
            var tutorialPage = (TutorialPage) GlobalUIManager.instance.currentPage;
            if (m_IsPractice)
            {
                tutorialPage.StartPracticeGame();
                return;
            }

            tutorialPage.StartNormalGame();
            return;
        }

        if (m_ChoiceType == LevelChoiceType.IngameObjective)
        {
            MiniGameController.current.HandleCallObjectiveReached(player);
            return;
        }

        if (m_ChoiceType == LevelChoiceType.TriggerObjective)
        {
            MiniGameController.current.HandleCallObjectiveReached(player);
            return;
        }
    }

    private IEnumerator DelayedActivation()
    {
        yield return new WaitForSeconds(1.6f);
        isActive = true;
    }

    #region EditorHelpers

    public bool isMode => m_ChoiceType == LevelChoiceType.Mode;
    public bool isSave => m_ChoiceType == LevelChoiceType.Save;
    public bool isMiniGameMode => m_ChoiceType == LevelChoiceType.MiniGameMode;
    public bool isPlayers => m_ChoiceType == LevelChoiceType.NumberOfPlayers;

    #endregion
}

public enum LevelChoiceType
{
    Save,
    MiniGameMode,
    Mode,
    NumberOfPlayers,
    IngameObjective,
    TriggerObjective,
    RestartGame,
    QuitGame
}