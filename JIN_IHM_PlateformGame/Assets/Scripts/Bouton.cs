using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using UnityEngine.UI;

public class Bouton : MonoBehaviour
{
   [SerializeField]
    private SerialHandler _serialHandler;
    [SerializeField]
    private Image _timer;
    [SerializeField]
    private float _timerTime;
    [SerializeField]
    private GameObject UIimage;

    private bool _ledIsOn = false;
    private float time;
    bool timeClicking;

    private void Update()
    {
        if ( timeClicking )
        {
            time -= Time.deltaTime;
            _timer.fillAmount = time / _timerTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!_ledIsOn)
                changeLed();
        }
    }

    private void changeLed()
    {
        _serialHandler.messageInterne = (_ledIsOn ? "active" : "inactive");
        _serialHandler.SetLed(_ledIsOn = !_ledIsOn);
        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, (_ledIsOn ? -0.3f : 0), transform.localPosition.z);
    }

    private IEnumerator timerCoroutine()
    {
        timeClicking = true;
        UIimage.SetActive(true);
        time = _timerTime;
        yield return new WaitForSeconds(_timerTime);
        changeLed();
        timeClicking = false;
        UIimage.SetActive(false);
    }
}
