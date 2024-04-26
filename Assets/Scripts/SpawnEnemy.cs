using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public GameObject spherePrefab; // Reference to the sphere prefab
    public List<Transform> spawnPoints;

    private float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (spherePrefab != null && spawnPoints != null && spawnPoints.Count > 0)
        {
            foreach (Transform spawnPoint in spawnPoints)
            {
                Instantiate(spherePrefab, spawnPoint.position, Quaternion.identity);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer++;
        if (timer > 2000f)
        {
            foreach (Transform spawnPoint in spawnPoints)
            {
                Instantiate(spherePrefab, spawnPoint.position, Quaternion.identity);
            }
            timer = 0;
        }
    }
}
