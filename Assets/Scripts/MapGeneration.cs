using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGeneration : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector3 spawn;
    [SerializeField] private GameObject[] structures;
    [SerializeField] private GameObject path;
    [SerializeField] private int maxStructures;

    private Vector3 current;
    private List<GameObject> buildings;

    void Start()
    {
        this.current = spawn;
        this.buildings = new List<GameObject>();
    }

    void Update()
    {
        if (buildings.Count < maxStructures)
        {
            GameObject toSpawn = structures[Random.Range(0, structures.Length)];
            UpdateCurrent(toSpawn.GetComponent<BoxCollider>());

            GameObject spawnedObj = Instantiate(toSpawn, current, new Quaternion(0, 0, 0, 0));
            buildings.Add(spawnedObj);
        }
    }

    void UpdateCurrent(BoxCollider collider)
    {
        // x offset
        this.current.x += collider.size.x; 
    }

    bool Chance(float perc)
    {
        return Random.Range(0.0f, 1.0f) < perc;
    }
}
