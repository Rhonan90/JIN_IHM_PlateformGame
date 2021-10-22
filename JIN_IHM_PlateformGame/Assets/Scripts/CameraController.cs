using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    //private float timeToFollow = 0.02f;
    public Vector2 Offset;
    public float cameraDistance;

    void Update()
    {
        //transform.position = new Vector3(Mathf.Lerp(transform.position.x,player.transform.position.x,timeToFollow)+Offset.x, Mathf.Lerp(transform.position.y, player.transform.position.y, timeToFollow)+Offset.y, this.transform.position.z);
    }

}
