using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class UIPage : SerializedMonoBehaviour
{
    public GameScreen screen;
    [SerializeField] protected GameObject m_SceneObject;

    private Animator m_Animator;
    private static readonly int CloseAnim = Animator.StringToHash("Close");
    private static readonly int OpenAnim = Animator.StringToHash("Open");

    private Animator animator
    {
        get
        {
            if (m_Animator == null)
                m_Animator = m_SceneObject.GetComponent<Animator>();
            return m_Animator;
        }
    }

    public virtual void Open()
    {
//        Debug.Log("Open page  "+screen);
        gameObject.SetActive(true);
        if (m_SceneObject)
        {
            m_SceneObject.SetActive(true);
            animator.SetTrigger(OpenAnim);
        }
    }


    public virtual void Close()
    {
//        Debug.Log("Close page  "+screen);
        if (screen == GameScreen.MainMenu)
        {
            gameObject.SetActive(false);
            if (m_SceneObject)
            {
                m_SceneObject.SetActive(false);
            }
            return;
        }

        StartCoroutine(CloseDelayAnimation());
    }

    public virtual IEnumerator CloseDelayAnimation()
    {
        if (m_SceneObject)
        {
            animator.SetTrigger(CloseAnim);
        }

        yield return new WaitForSeconds(1.3f);
        gameObject.SetActive(false);
        if (m_SceneObject)
        {
            m_SceneObject.SetActive(false);
        }
    }
    
    
    public void HidePage()
    {
        gameObject.SetActive(false);
        if(m_SceneObject)
            m_SceneObject.SetActive(false);
    }
    public void Show()
    {
        gameObject.SetActive(true);
        if(m_SceneObject)
            m_SceneObject.SetActive(true);
    }
}