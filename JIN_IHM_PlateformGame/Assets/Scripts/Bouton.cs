using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

public class Bouton : MonoBehaviour
{
    [SerializeField]
    private SerialHandler _serialHandler;

    private bool _ledIsOn = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _serialHandler.SetLed( _ledIsOn = !_ledIsOn);
        }
    }
}
