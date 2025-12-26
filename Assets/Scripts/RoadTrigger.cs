using UnityEngine;

public class RoadTrigger : MonoBehaviour
{
    private RoadSegment roadSegment;
    public RoadManager roadManager;
    // Start is called before the first frame update
    void Start()
    {
        roadSegment = GetComponentInParent<RoadSegment>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            roadManager.OnEnterNewRoadSegment(roadSegment.segmentIndex);
        }
    }
}
