using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelEdgeDetector : MonoBehaviour
{

    public float detectionRange = 10f;
    public float distanceToGoalDetectionRange = 100f;

    public int numberOfEdges = 36;
    public GameObject edgeDetectorVisualizerPrefab;
    List<Edge> edges = new List<Edge>();

    public Transform parent;
    public Transform visualizationParent;
    public Transform target;

    public bool drawEdges = true;

    //We only want to hit colliders on the wall layer
    int wallMask = 0;
    int enemyMask = 0;
    int finalMask = 0;

    float closestDistanceToGoal = 999999;

    // Use this for initialization
    void Start()
    {
        for (int x = 0;x < numberOfEdges; x++)
        {
            GameObject edgeVisualizer = Instantiate(edgeDetectorVisualizerPrefab, visualizationParent);
            edgeVisualizer.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, x * (360f / numberOfEdges) -90f));
            edges.Add(edgeVisualizer.GetComponent<Edge>());
        }
    }

    public void setTeam(int teamNumber)
    {
        wallMask = 1 << LayerMask.NameToLayer("Raycastable");

        int enemyLayer = teamNumber == 0 ? 1 : 0;
        enemyMask = 1 << LayerMask.NameToLayer("Team" + enemyLayer);

        finalMask = wallMask | enemyMask;
    }

    /// <summary>
    /// Returns 8 #'s between -1 to 1 based on the distance of each edge
    /// </summary>
    /// <returns></returns>
    public List<float> getEdges()
    {
        List<float> returnList = new List<float>();
        float delta = 360f / numberOfEdges;

        for(float x = 0;x < numberOfEdges; x++)
        {
            Vector3 direction = new Vector3(Mathf.Cos(Mathf.Deg2Rad * (x*delta)), Mathf.Sin(Mathf.Deg2Rad * (x * delta)), 0);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(direction.x, direction.y), detectionRange, finalMask);
            float distance = hit ? normalizePoint(hit) : 1;
            returnList.Add(distance);
            //ui stuff
            if (drawEdges)
            {
                edges[(int)x].setSize((0.5f + (distance / 2f)) * detectionRange, detectionRange);//need to first turn our -1 - 1 to 0 - 1
            }
        }


        return returnList;

        //int index = 0;
        //for (float x = -1; x < 2; x += 1)
        //{
        //    for (float y = -1; y < 2; y += 1)
        //    {
        //        if (x == 0 && y == 0)
        //        {
        //            continue;
        //        }
        //        //make the requests from each direction

        //        Vector2 xDirection = transform.right * x;
        //        Vector2 yDirection = transform.up * y;

        //        RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(xDirection.x + yDirection.x, xDirection.y + yDirection.y), detectionRange, raycastableMask);
        //        //RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(x*transform.right.x, y*transform.up.y), detectionRange, raycastableMask);

        //        float distance = hit ? normalizePoint(hit) : 1;
        //        returnList.Add(distance);
        //        //ui stuff
        //        if (drawEdges)
        //        {
        //            edges[index].setSize((0.5f + (distance / 2f)) * detectionRange, detectionRange);//need to first turn our -1 - 1 to 0 - 1
        //        }

        //        index++;
        //    }
        //}
        return returnList;
    }

    /// <summary>
    /// Returns either -1 or 1 if the ship can see the goal
    /// </summary>
    /// <returns></returns>
    public float getLOS()
    {
        Vector2 heading = target.position - transform.position;
        heading /= heading.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, heading, 1000, finalMask);

        return hit && hit.collider.CompareTag("Target") ? 1 : -1;
    }

    /// <summary>
    /// Returns between 0-1 for each axis, normalized in the goalDetectionRange. 0.5f is close, 1 and 0 or far on each side
    /// </summary>
    /// <returns></returns>
    public List<float> getXYDistancesToGoal()
    {
        float xDistance = target.position.x - transform.position.x;
        float yDistance = target.position.y - transform.position.y;

        float distanceToGoal = (target.position - transform.position).magnitude;
        if (distanceToGoal < closestDistanceToGoal)
        {
            closestDistanceToGoal = distanceToGoal;

        }


        return new List<float>() { normalizeDistanceToGoal(xDistance), normalizeDistanceToGoal(yDistance) };
    }

    public float getClosestDitanceToGoal()
    {
        return closestDistanceToGoal;
    }

    public void resetClosestDistance()
    {
        closestDistanceToGoal = 99999;
    }

    float normalizeDistanceToGoal(float input)
    {
        if (input > 0)
        {
            float high = (input / distanceToGoalDetectionRange);
            return high > 1f ? 1f : high;
        }

        float low = 0f - Mathf.Abs(input / distanceToGoalDetectionRange);
        return low < 0.0f ? 0.0f : low;
    }

    /// <summary>
    /// Gets a number between -1 to 1 based on detection range
    /// </summary>
    /// <param name="hit"></param>
    /// <returns></returns>
    float normalizePoint(RaycastHit2D hit)
    {
        if (!hit)
        {
            return 1f;
        }
        return hit.distance > detectionRange ? 1f : (-1f + 2f * (hit.distance / detectionRange));
    }
}
