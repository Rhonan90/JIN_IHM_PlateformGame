using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public float timeToFollow;

    void Update()
    {
        transform.position = new Vector3(Mathf.Lerp(transform.position.x,player.transform.position.x,timeToFollow), Mathf.Lerp(transform.position.y, player.transform.position.y, timeToFollow), this.transform.position.z);
    }
}
