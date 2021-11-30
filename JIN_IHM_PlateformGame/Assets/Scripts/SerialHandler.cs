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


    [SerializeField] private GameObject[][] obstacles;
    [SerializeField] private Colors[] colors;

    [Range(0, 255)]
    [SerializeField] private int inactiveObstacleColorAlpha;
    private BoxCollider2D[][] _obstacleBoxCollider2D;
    private SpriteRenderer[][] _obstacleSprite;

    // Start is called before the first frame update
    void Start()
    {
        _serial = new SerialPort(serialPort, baudrate);
        // Once configured, the serial communication must be opened just like a file : the OS handles the communication.
        _serial.Open();

        for (int j = 0; j < colors.Length; j++)
        {
            for (int i = 0; i < colors[j].obstacle.Length; i++)
            {
                _obstacleBoxCollider2D[j] = new BoxCollider2D[colors.Length];
                _obstacleSprite[j] = new SpriteRenderer[colors.Length];
            }
        }


        for (int j = 0; j < obstacles.Length; j++)
        {
            for (int i = 0; i < obstacles[j].Length; i++)
            {
                _obstacleBoxCollider2D[j][i] = colors[j].obstacle[i].gameObject.transform.GetComponent<BoxCollider2D>();
                _obstacleSprite[j][i] = colors[j].obstacle[i].gameObject.transform.GetComponent<SpriteRenderer>();
                _obstacleSprite[j][i].color = new Color32(colors[j].color.r, colors[j].color.g, colors[j].color.b, ((byte)inactiveObstacleColorAlpha));
                _obstacleBoxCollider2D[j][i].isTrigger = true;
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
        char[] number = new char[1];
        int color = (int)Char.GetNumericValue(number[0]);

        switch (message.TrimEnd(number))
        {
            case "inactive":
                    for (int i = 0; i < colors[color].obstacle.Length; i++)
                    {
                        _obstacleBoxCollider2D[color][i].isTrigger = true;
                        _obstacleSprite[color][i].color = new Color32(colors[color].color.r, colors[color].color.g, colors[color].color.b, ((byte)inactiveObstacleColorAlpha));
                    }
                SetLed(false);
                break;
            case "active":
                    for (int i = 0; i < colors[color].obstacle.Length; i++)
                    {
                        _obstacleBoxCollider2D[color][i].isTrigger = false;
                        _obstacleSprite[color][i].color = new Color32(colors[color].color.r, colors[color].color.g, colors[color].color.b, ((byte)inactiveObstacleColorAlpha));
                    }             
                SetLed(true);
                break;
        }
    }

    public void SetLed(bool newState)
    {
        _serial.WriteLine(newState ? "LED ON" : "LED OFF");
    }

    private void OnDestroy()
    {
        _serial.Close();
    }
}
