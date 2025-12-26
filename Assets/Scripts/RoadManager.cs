using System.Collections.Generic;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    public List<RoadSegment> roadSegments;
     private int currentRoadIndex = 0;
      private float leftmostRoadX;
    // Start is called before the first frame update
    void Start()
    {
        UpdateLeftmostRoadX();
        InitAllRoadObstacles();
    }

    private void InitAllRoadObstacles()
    {
        foreach (RoadSegment road in roadSegments)
        {
            road.SpawnObstacles();
        }
    }

    public void OnEnterNewRoadSegment(int newSegmentIndex)
    {
        if (!GameManager.Instance.isGameRunning)
        {
            return; //GameOver
        }

        if (newSegmentIndex != currentRoadIndex)
        {
            currentRoadIndex = newSegmentIndex;
            int roadToMoveIndex = (currentRoadIndex - 1 + roadSegments.Count) % roadSegments.Count;
            MoveRoadToFront(roadToMoveIndex);
        }
    }

    private void MoveRoadToFront(int roadIndex)
    {
        RoadSegment roadToMove = roadSegments[roadIndex];
        float newX = leftmostRoadX - roadToMove.segmentLength;
        Vector3 newPos = new Vector3(newX, roadToMove.transform.position.y, roadToMove.transform.position.z);
        roadToMove.transform.position = newPos;

        UpdateLeftmostRoadX();

        roadToMove.SpawnObstacles();
    }

    private void UpdateLeftmostRoadX()
    {
        leftmostRoadX = Mathf.Min(roadSegments[0].transform.position.x, 
                                  roadSegments[1].transform.position.x, 
                                  roadSegments[2].transform.position.x);
    }
}
