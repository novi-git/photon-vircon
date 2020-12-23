using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlimpMovement : MonoBehaviour
{
    public float moveSpeed = 10;
    public float turnSpeed = 30; 
    public float closeDistance = 2.0f; 
    public Transform wayPointsParent;

    private int currentWaypointIndex = 0;
    private Vector3[] wayPointsChild; 
    private Quaternion qTo;

    // Start is called before the first frame update
    void Start()
    {
        wayPointsChild = new Vector3[wayPointsParent.childCount];
        for(int i = 0; i < wayPointsParent.childCount; i++){
            wayPointsChild[i] = wayPointsParent.GetChild(i).transform.position;
        }
        //transform.position = wayPointsChild[currentWaypointIndex]; 
        qTo = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    { 
            Vector3 direction = wayPointsChild[currentWaypointIndex] - transform.position;
            float sqrLen = direction.sqrMagnitude; 
            Vector3 rotationDir = new Vector3(direction.x, 0.0f, direction.z);
            qTo = Quaternion.LookRotation (rotationDir);
            transform.rotation = Quaternion.Slerp (transform.rotation, qTo, Time.deltaTime * turnSpeed);
 
            transform.position = Vector3.MoveTowards(transform.position, wayPointsChild[currentWaypointIndex], moveSpeed * Time.deltaTime);


              if (sqrLen < closeDistance * closeDistance)
            {  
                  currentWaypointIndex++;
                if(currentWaypointIndex >= wayPointsChild.Length){
                    currentWaypointIndex = 0; // reset
                }
            }  
            
    }
}
