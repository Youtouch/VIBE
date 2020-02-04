using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubePattern : MonoBehaviour
{
    private int m_CompletionSize = -1;
    private int m_CurrentChild = 0;

    public int team, patternId;
    private Transform childHodler;

    public Animation m_TopCastle;
    public GameObject dustVFX;

    private void Start()
    {
        m_TopCastle.gameObject.SetActive(false);
        childHodler = transform.GetChild(0);
        foreach (var child in childHodler)
        {
            var childTransform = (Transform) child;
            childTransform.gameObject.SetActive(false);
        }
    }

    public int completionRequirements
    {
        get
        {
            if (m_CompletionSize == -1)
                m_CompletionSize = childHodler.childCount;
            return m_CompletionSize;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag($"Cube"))
        {
//            Debug.Log("Cube entered messqge");
            PickupController cube = other.gameObject.GetComponent<PickupController>();
            if (cube.lastOwner != null && cube.lastOwner.team == team)
            {
//                Debug.Log("Handle cube entered");
                PlayerManager.instance.TrackerUpdate(cube.lastOwner, PlayerTrackerVariable.build);
                if (m_CurrentChild < completionRequirements)
                {
                    MiniGameController.current.GivePoints(10, cube.lastOwner.idInManager);
                }
                cube.lastOwner.Drop();
                Destroy(other.gameObject);
                HandleCubeReception();
            }
        }
    }

    private void HandleCubeReception()
    {
        if (m_CurrentChild >= completionRequirements)
        {
            Debug.Log("Handle cube");
            m_TopCastle.gameObject.SetActive(false);
            StartCoroutine(DelayWin());
            return;
        }

        childHodler.GetChild(m_CurrentChild).gameObject.SetActive(true);
        GameObject gm = Instantiate(dustVFX);
        Destroy(gm, 2);
        gm.transform.position = childHodler.GetChild(m_CurrentChild).position;
        m_CurrentChild++;
    }

    IEnumerator DelayWin()
    {
        m_TopCastle.gameObject.SetActive(true);
        m_TopCastle.Play();
        yield return (1.5f);
        MiniGameController.current.OnPatternMessageReceived(this);
    }

    public void Reset()
    {
        gameObject.SetActive(false);
        for (int i = 0; i <= m_CurrentChild; i++)
        {
            if (i < completionRequirements)
                childHodler.GetChild(i).gameObject.SetActive(false);
        }

        m_CurrentChild = 0;
    }
}