using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [SerializeField] private int spawnOnPress = 50;
    [SerializeField] private float timeBetweenSpawns = 0.1f;
    [SerializeField] private float jitter = 0.01f;
    [SerializeField] private float sizeJitter = 0.3f;

    [SerializeField] private List<Debris> debrisPrefabs;
    
    private int _spawnsLeft = 0;
    
    private float _lastSpawnTime = 0;
    
    private void FixedUpdate()
    {
        if (_spawnsLeft > 0 && TimeSinceLastSpawn > timeBetweenSpawns)
        {
            SpawnRandom();
            _spawnsLeft--;
            
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

    private void SpawnRandom()
    {
        if (debrisPrefabs is not {Count: > 0}) return;
        
        var debrisPrefab = debrisPrefabs[Random.Range(0, debrisPrefabs.Count)];
        if (!debrisPrefab) return;
        
        var newDeb = Instantiate(debrisPrefab, transform);
        
        newDeb.transform.localPosition += new Vector3(Random.Range(-jitter, jitter), 0, Random.Range(-jitter, jitter));
        newDeb.transform.localScale += new Vector3(
            Random.Range(-sizeJitter, sizeJitter),
            Random.Range(-sizeJitter, sizeJitter),
            Random.Range(-sizeJitter, sizeJitter));
        
        _lastSpawnTime = Time.time;
    }
}
