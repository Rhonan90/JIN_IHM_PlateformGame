using System;
using System.IO;
using System.IO.Ports;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SerialHandler : MonoBehaviour
{

    private SerialPort _serial;

    // Common default serial device on a Windows machine
    [SerializeField] private string serialPort = "COM1";
    [SerializeField] private int baudrate = 9600;


    [SerializeField] private Colors[] colors;

    [Range(0, 255)]
    [SerializeField] private int activeObstacleColorAlpha;
    [Range(0, 255)]
    [SerializeField] private int inactiveObstacleColorAlpha;
    private BoxCollider2D[][] _obstacleBoxCollider2D;
    private SpriteRenderer[][] _obstacleSprite;

    public string messageInterne = null;

    // Start is called before the first frame update
    void Start()
    {
        _serial = new SerialPort(serialPort, baudrate);
        // Once configured, the serial communication must be opened just like a file : the OS handles the communication.
        _serial.Open();

        _obstacleBoxCollider2D = new BoxCollider2D[colors.Length][];
        _obstacleSprite = new SpriteRenderer[colors.Length][];
        for (int j = 0; j < colors.Length; j++)
        {
            _obstacleBoxCollider2D[j] = new BoxCollider2D[colors[j].obstacle.Length];
            _obstacleSprite[j] = new SpriteRenderer[colors[j].obstacle.Length];
        }


        for (int j = 0; j < colors.Length; j++)
        {
            for (int i = 0; i < colors[j].obstacle.Length; i++)
            {
                _obstacleBoxCollider2D[j][i] = colors[j].obstacle[i].gameObject.transform.GetComponent<BoxCollider2D>();
                _obstacleBoxCollider2D[j][i].isTrigger = true;
                _obstacleSprite[j][i] = colors[j].obstacle[i].gameObject.transform.GetComponent<SpriteRenderer>();
                _obstacleSprite[j][i].color = new Color32(colors[j].color.r, colors[j].color.g, colors[j].color.b, (_obstacleBoxCollider2D[j][i].isTrigger ? (byte)inactiveObstacleColorAlpha : (byte)activeObstacleColorAlpha));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Prevent blocking if no message is available as we are not doing anything else
        // Alternative solutions : set a timeout, read messages in another thread, coroutines, futures...
        if (_serial.BytesToRead <= 0) return;



        var message = _serial.ReadLine();


        // Arduino sends "\r\n" with println, ReadLine() removes Environment.NewLine which will not be 
        // enough on Linux/MacOS.
        if (Environment.NewLine == "\n")
        {
            message = message.Trim('\r');
        }
        var number = message[message.Length - 1];
        message = message.Remove(message.Length - 1);
        int color = (int)Char.GetNumericValue(number);

        if (messageInterne != null)
        {
            message = messageInterne;
            color = colors.Length - 1;
        }

        switch (message)
        {
            case "inactive":
                    for (int i = 0; i < colors[color].obstacle.Length; i++)
                    {
                        _obstacleBoxCollider2D[color][i].isTrigger = true;
                        _obstacleSprite[color][i].color = new Color32(colors[color].color.r, colors[color].color.g, colors[color].color.b, ((byte)inactiveObstacleColorAlpha));
                    }
                break;
            case "active":
                    for (int i = 0; i < colors[color].obstacle.Length; i++)
                    {
                        _obstacleBoxCollider2D[color][i].isTrigger = false;
                        _obstacleSprite[color][i].color = new Color32(colors[color].color.r, colors[color].color.g, colors[color].color.b, ((byte)activeObstacleColorAlpha));
                    }
                break;
        }
        messageInterne = null;
    }

    public void SetLed(bool newState)
    {
        _serial.WriteLine(newState ? "notifyActiveBlue" : "notifyInactiveBlue");
    }

    private void OnDestroy()
    {
        _serial.Close();
    }
}
