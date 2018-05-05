using System;
using UnityEngine;
using UnityEngine.UI;

public class MainCanvasScript : MonoBehaviour
{
    private int _timer;
    private Text _timerText;

    private Transform _healthBarTransform;

    private void Awake()
    {
        _timerText = transform.Find("Timer").GetComponent<Text>();
        _healthBarTransform = transform.Find("HealthBarForeground").transform;

        InvokeRepeating(nameof(UpdateTime), 1f, 1f);
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
