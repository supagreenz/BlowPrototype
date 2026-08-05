using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [SerializeField] private int spawnOnPress = 50;
    [SerializeField] private float timeBetweenSpawns = 0.1f;

    [SerializeField] private List<Debris> debrisPrefabs;
    
    private int _spawnsLeft = 0;
    
    private float _lastSpawnTime = 0;
    
    private void FixedUpdate()
    {
        if (_spawnsLeft > 0 && TimeSinceLastSpawn > timeBetweenSpawns)
        {
            if (TrySpawnRandom()) _spawnsLeft--;
        }
    }

    private void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (_spawnsLeft <= 0) _spawnsLeft = spawnOnPress;
        }
    }
    
    private float TimeSinceLastSpawn => Time.time - _lastSpawnTime;

    private bool TrySpawnRandom()
    {
        if (debrisPrefabs is not {Count: > 0}) return false;
        
        var debrisPrefab = debrisPrefabs[Random.Range(0, debrisPrefabs.Count)];
        if (!debrisPrefab) return false;
        
        var newDeb = Instantiate(debrisPrefab, transform);
        
        _lastSpawnTime = Time.time;
        return true;
    }
}
