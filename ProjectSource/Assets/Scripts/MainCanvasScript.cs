using System;
using UnityEngine;
using UnityEngine.UI;

public class MainCanvasScript : MonoBehaviour
{
    private int _timer;
    private Text _timerText;
    private Text _fpsText;

    private Transform _healthBarTransform;
    private int _frameCount;
    private float _deltaTimeSum;
    private float _fpsTextUpdateRate = 4f;

    private void Awake()
    {
        _timerText = transform.Find("Timer").GetComponent<Text>();
        _healthBarTransform = transform.Find("HealthBarForeground").transform;
        _fpsText = transform.Find("FPS").GetComponent<Text>();

        InvokeRepeating(nameof(UpdateTime), 1f, 1f);
    }

    private void Update()
    {
        _frameCount++;
        _deltaTimeSum += Time.deltaTime;
        if (_deltaTimeSum > 1f / _fpsTextUpdateRate)
        {
            int fps = Mathf.FloorToInt(_frameCount / _deltaTimeSum);
            _fpsText.text = $"{fps}";
            _frameCount = 0;
            _deltaTimeSum -= 1f / _fpsTextUpdateRate;
        }
    }

    public void SetHealthPercentage(float healthPercentage)
    {
        if (float.IsNaN(healthPercentage))
        {
            throw new ArgumentException("The provided health percentage was NaN.");
        }

        if (healthPercentage < 0f || healthPercentage > 1f)
        {
            throw new ArgumentOutOfRangeException("The provided health percentage must be between 0 and 1 inclusive, " +
                                                  $"but was {healthPercentage}.");
        }

        _healthBarTransform.localScale = new Vector3(healthPercentage, 1f, 1f);
    }

    private void UpdateTime()
    {
        _timer++;
        _timerText.text = $"{_timer}";
    }
}
