using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [SerializeField] private int spawnOnPress = 50;
    [SerializeField] private float timeBetweenSpawns = 0.1f;
    [SerializeField] private float jitter = 0.01f;

    [SerializeField] private Debris debrisPrefab;
    
    private int _spawnsLeft = 0;
    
    private float _lastSpawnTime = 0;
    
    private void FixedUpdate()
    {
        if (_spawnsLeft > 0 && TimeSinceLastSpawn > timeBetweenSpawns)
        {
            SpawnOne();
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

    private void SpawnOne()
    {
        if (!debrisPrefab) return;
        
        var newDeb = Instantiate(debrisPrefab, transform);
        
        newDeb.transform.localPosition = new Vector3(Random.Range(-jitter, jitter), 0, Random.Range(-jitter, jitter));
        
        _lastSpawnTime = Time.time;
    }
}
