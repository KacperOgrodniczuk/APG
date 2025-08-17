using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    public GameObject block;

    int blocksSpawned = 0;

    public void SpawnBlock()
    {
        if (blocksSpawned < 5)
        {
            Instantiate(block, transform.position, Quaternion.identity);
            blocksSpawned++;
        }
    }
}
