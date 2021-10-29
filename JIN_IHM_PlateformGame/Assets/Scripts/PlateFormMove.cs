using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateFormMove : MonoBehaviour
{
    public float speed;
    public float timeOfMovement=2;
    private float time = 0;

    // Update is called once per frame
    void Update()
    {
        if (timeOfMovement - time < 0)
        {
            speed = -speed;
            time = 0;
        }
        transform.position += new Vector3( speed * Time.deltaTime,0,0);
        time+=Time.deltaTime;
    }

    public float getPlateformSpeed()
    {
        return speed;
    }
}
