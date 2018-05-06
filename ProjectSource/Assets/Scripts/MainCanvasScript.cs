using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainCanvasScript : MonoBehaviour
{
    private int _timer;
    private Text _timerText;
    private Text _fpsText;
    private Text _youLoseText;
    private Text _youWinText;

    private Transform _healthBarTransform;
    private int _frameCount;
    private float _deltaTimeSum;
    private float _fpsTextUpdateRate = 4f;

    private List<Transform> _totems;

    private void Awake()
    {
        _timerText = transform.Find("Timer").GetComponent<Text>();
        _healthBarTransform = transform.Find("HealthBarForeground").transform;
        _fpsText = transform.Find("FPS").GetComponent<Text>();
        _youLoseText = transform.Find("YouLoseText").GetComponent<Text>();
        _youWinText = transform.Find("YouWinText").GetComponent<Text>();

        _totems = new List<Transform>();
        foreach (GameObject totem in GameObject.FindGameObjectsWithTag("Totem"))
        {
            _totems.Add(totem.transform);
        }

        InvokeRepeating(nameof(UpdateTime), 1f, 1f);
        InvokeRepeating(nameof(ScanTotems), 2f, 2f);
    }

    private void ScanTotems()
    {
        bool win = true;
        Vector3 position = _totems[0].position;
        for (int i = 1; i < 4; i++)
        {
            float distance = Vector2.Distance(position, _totems[i].position);
            win = win && (distance < 5.5f);
        }

        if (win)
        {
            _youWinText.enabled = true;
            Invoke("RestartGame", 3f);
        }
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

        if (healthPercentage < 0.01f)
        {
            PlayerDied();
        }

        _healthBarTransform.localScale = new Vector3(healthPercentage, 1f, 1f);
    }

    private void UpdateTime()
    {
        _timer++;
        _timerText.text = $"{_timer}";
    }

    private void PlayerDied()
    {
        _youLoseText.enabled = true;
        Invoke("RestartGame", 3f);

    }

    private void RestartGame()
    {
        SceneManager.LoadScene(0);
    }
}
