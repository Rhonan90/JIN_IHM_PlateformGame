using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private Engine engine;
    private PlayerAvatar playerAvatar;

    void Start()
    {
        engine = GetComponent<Engine>();
        playerAvatar = GetComponent<PlayerAvatar>();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalSpeedPercent = Input.GetAxis("Horizontal");
        float maxSpeed = playerAvatar.MaxSpeed;

        engine.speed.x = maxSpeed * horizontalSpeedPercent;
    }
}
