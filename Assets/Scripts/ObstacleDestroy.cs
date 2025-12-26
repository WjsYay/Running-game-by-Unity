using UnityEngine;

public class ObstacleDestroy : MonoBehaviour
{
    public float destroyDistance = 30f;

    // Update is called once per frame
    void Update()
    {
        GameObject[] allObstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject obs in allObstacles)
        {
            
            if (obs.transform.position.x > transform.position.x + destroyDistance)
            {
                Destroy(obs);
            }
        }
    }
}
