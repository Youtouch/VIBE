using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraHandler : SingletonBehaviour<CameraHandler>
{
    private Vector3 m_BasePosition;
    [SerializeField] private CameraScrollingType m_ScrollingType;
    [SerializeField] private float focusSpeed;
    [SerializeField] private CameraPositionToDistance minDistance, maxDistance;

    public static void UpdateScrollingType(CameraScrollingType newType)
    {
        instance.m_ScrollingType = newType;
        if (newType == CameraScrollingType.None)
            instance.transform.position = Vector3.zero;
    }

    private void Update()
    {
        if (GameManager.instance.state == GameState.Playing && m_ScrollingType == CameraScrollingType.FollowPlayers)
        {
            HandleFollowPlayers();
        }
    }

    private void HandleFollowPlayers()
    {
        var maxPlayerDistance = 0f;
        foreach (var distance in PlayerManager.players.SelectMany(player =>
            PlayerManager.players.Select(otherPlayer => CalculateDistance(player.transform, otherPlayer.transform)).Where(distance => distance > maxPlayerDistance)))
        {
            maxPlayerDistance = distance;
        }

        var ratio = maxPlayerDistance / Mathf.Abs(maxDistance.distance - minDistance.distance);
        var newCameraPositionZ = maxDistance.position + (ratio / Mathf.Abs(maxDistance.position - minDistance.position));
        var currentPosition = transform.position;

        if (maxPlayerDistance <= minDistance.distance)
            newCameraPositionZ = minDistance.position;
        if (maxPlayerDistance >= maxDistance.distance)
            newCameraPositionZ = maxDistance.position;
        
        var newPosition = new Vector3(currentPosition.x, currentPosition.y, newCameraPositionZ);

        transform.position = Vector3.Lerp(currentPosition, newPosition, focusSpeed);
    }

    private float CalculateDistance(Transform entity_1, Transform entity_2)
    {
        return Vector3.Distance(entity_1.position, entity_2.position);
    }

    private void OnTriggerExit(Collider other)
    {
//        Debug.Log("SOMEONE ENTERED");
        if(other.gameObject.CompareTag("Player"))
        {
            //GERER LA MORT DANS LE PLAYER
            other.gameObject.GetComponent<PlayerController>().gameObject.SetActive(false);
            if(MiniGameController.current)
            {
                MiniGameController.current.HandleCallPlayerDied();
                return;
            }
            PlayerManager.instance.RevivePlayers();
        }
        if(other.gameObject.CompareTag("Cube"))
        {
            other.transform.position = Vector3.up * 2;
        }
    }
}

public struct CameraPositionToDistance
{
    public float position, distance;
}