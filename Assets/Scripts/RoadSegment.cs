using UnityEngine;
using System.Collections.Generic;

public class RoadSegment : MonoBehaviour
{
    //道路编号
    public int segmentIndex;
    public float segmentLength = 30f;

    public List<GameObject> obstaclePrefabs; 
    private List<GameObject> currentObstacles = new List<GameObject>(); 
    private readonly float[] obstacleZPositions = new float[] { -3f, 0f, 3f };


    // 在道路上生成障碍物
    public void SpawnObstacles()
    {

        ClearObstacles();

        if (obstaclePrefabs == null || obstaclePrefabs.Count == 0)
        {
            Debug.LogWarning("道路" + segmentIndex + "的障碍物预制体列表为空！");
            return;
        }


        int randomPrefabIndex = Random.Range(0, obstaclePrefabs.Count);
        GameObject selectedObstaclePrefab = obstaclePrefabs[randomPrefabIndex];

        int randomPosIndex = Random.Range(0, obstacleZPositions.Length);
        float targetZ = obstacleZPositions[randomPosIndex];

        Vector3 spawnPos = new Vector3(
            transform.position.x - segmentLength / 2, 
            transform.position.y,
            targetZ
        );

        GameObject obstacle = Instantiate(selectedObstaclePrefab, spawnPos, Quaternion.identity);
        obstacle.transform.parent = transform;
        currentObstacles.Add(obstacle); 

        if (Random.value > 0.5f) 
        {
            int anotherPrefabIndex = (randomPrefabIndex + 1) % obstaclePrefabs.Count;
            GameObject anotherObstaclePrefab = obstaclePrefabs[anotherPrefabIndex];
            int anotherPosIndex = (randomPosIndex + 1) % obstacleZPositions.Length;
            float anotherTargetZ = obstacleZPositions[anotherPosIndex];
            Vector3 anotherSpawnPos = new Vector3(
                transform.position.x - segmentLength / 2 + Random.Range(-5f, 5f), 
                transform.position.y,
                anotherTargetZ
            );
            GameObject anotherObstacle = Instantiate(anotherObstaclePrefab, anotherSpawnPos, Quaternion.identity);
            anotherObstacle.transform.parent = transform;
            currentObstacles.Add(anotherObstacle);
        }
    }

    //清除障碍物
    public void ClearObstacles()
    {
        foreach (GameObject obs in currentObstacles)
        {
            if (obs != null)
            {
                Destroy(obs);
            }
        }
        currentObstacles.Clear();
    }
}