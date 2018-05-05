using UnityEngine;

public class PlayerDamageScript : MonoBehaviour
{
    [SerializeField] private int _startingHealth;
    [SerializeField] private int _damagePerHit;
    [SerializeField] private float _sheildRegenDelayTime;
    [SerializeField] private float _sheildRegenIterationTime;
    [SerializeField] private int _sheildRegenIterationAmount;

    private int _health;
    private MainCanvasScript _mainCanvasScript;

    private void Awake()
    {
        _mainCanvasScript = GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<MainCanvasScript>();
        _health = _startingHealth;
    }

    public void TakeDamage(int damage)
    {
        _health = Mathf.Max(_health - damage, 0);
        float healthPercentage = (float)_health / _startingHealth;
        _mainCanvasScript.SetHealthPercentage(healthPercentage);

        CancelInvoke();
        InvokeRepeating(nameof(RegenerateSheilds), _sheildRegenDelayTime, _sheildRegenIterationTime);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            TakeDamage(_damagePerHit);
        }
    }

    private void RegenerateSheilds()
    {
        _health = Mathf.Min(_health + _sheildRegenIterationAmount, _startingHealth);
        float healthPercentage = (float)_health / _startingHealth;
        _mainCanvasScript.SetHealthPercentage(healthPercentage);

        if (_health == _startingHealth)
        {
            CancelInvoke();
        }
    }
}
