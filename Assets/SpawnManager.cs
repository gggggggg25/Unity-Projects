using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Transform[] spawnPoints;

    private int nextIndex = 0;

    public Transform GetNextSpawn()
    {
        Transform spawn = spawnPoints[nextIndex];

        nextIndex++;
        if (nextIndex >= spawnPoints.Length)
            nextIndex = 0;

        return spawn;
    }
}