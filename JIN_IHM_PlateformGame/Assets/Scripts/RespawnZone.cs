using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnZone : MonoBehaviour
{
    public GameObject RespawnPoint;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        RespawnPoint.gameObject.transform.position = this.transform.position;
        Destroy(gameObject);
    }
}
