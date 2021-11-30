using System;
using System.IO;
using System.IO.Ports;
using UnityEngine;

public class SerialHandler : MonoBehaviour
{

    private SerialPort _serial;

    // Common default serial device on a Windows machine
    [SerializeField] private string serialPort = "COM1";
    [SerializeField] private int baudrate = 9600;

    [SerializeField] private Component[] obstacles;
    [SerializeField] private Color32 obstacleColor;
    [Range(0, 255)]
    [SerializeField] private int inactiveObstacleColorAlpha;
    private Rigidbody2D[] _obstacleRigidbody2D;
    private SpriteRenderer[] _obstacleSprite;

    // Start is called before the first frame update
    void Start()
    {
        _serial = new SerialPort(serialPort, baudrate);
        // Once configured, the serial communication must be opened just like a file : the OS handles the communication.
        _serial.Open();

        for (int i = 0; i < obstacles.Length; i++)
        {
            _obstacleRigidbody2D[i] = obstacles[i].GetComponentInParent<Rigidbody2D>();
            _obstacleSprite[i] = obstacles[i].GetComponentInParent<SpriteRenderer>();
            _obstacleSprite[i].color = new Color(obstacleColor.r, obstacleColor.g, obstacleColor.b, inactiveObstacleColorAlpha);
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

        switch (message)
        {
            case "inactive":
                for (int i = 0; i < obstacles.Length; i++)
                {
                    _obstacleRigidbody2D[i].simulated = false;
                    _obstacleSprite[i].color = new Color32(obstacleColor.r, obstacleColor.g, obstacleColor.b, ((byte)inactiveObstacleColorAlpha));
                }
                SetLed(false);
                break;
            case "active":
                for (int i = 0; i < obstacles.Length; i++)
                {
                    _obstacleRigidbody2D[i].simulated = true;
                    _obstacleSprite[i].color = new Color32(obstacleColor.r, obstacleColor.g, obstacleColor.b, 255);
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
