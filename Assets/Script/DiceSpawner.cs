using UnityEngine;

public class DiceSpawner : MonoBehaviour
{
    public GameObject dicePrefab;
    public Transform spawnPoint;

    // Update is called once per frame
    void Update()
    {
        // Example: Spawn dice when space key is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnDice();
        }
    }

    void SpawnDice()
    {
        if (dicePrefab != null && spawnPoint != null)
        {
            Instantiate(dicePrefab, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            Debug.LogError("Dice Prefab or Spawn Point is not assigned.");
        }
    }
}
