using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    public GameObject cubePrefab;

    public bool spawnOverTime = true;
    [ShowIf("spawnOverTime")] public float spawnDelay = 5f;
    public int maxChildren;


    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(SpawnOverDelay());
    }

    private IEnumerator SpawnOverDelay()
    {
        yield return new WaitForSeconds(spawnDelay);
        if (transform.childCount < maxChildren)
            Instantiate(cubePrefab,transform);
        StartCoroutine(SpawnOverDelay());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Instantiate(cubePrefab);
        }
    }
}