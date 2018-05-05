using UnityEngine;

public class PlayerDamageScript : MonoBehaviour
{
    [SerializeField] private int _initialSheilds;
    [SerializeField] private int _damagePerHit;
    [SerializeField] private float _sheildRegenDelayTime;
    [SerializeField] private float _sheildRegenIterationTime;
    [SerializeField] private int _sheildRegenIterationAmount;

    private int _sheildHealth;
    private MainCanvasScript _mainCanvasScript;

    private void Awake()
    {
        _mainCanvasScript = GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<MainCanvasScript>();
        _sheildHealth = _initialSheilds;
    }

    public void TakeDamage(int damage)
    {
        _sheildHealth = Mathf.Max(_sheildHealth - damage, 0);
        float healthPercentage = (float)_sheildHealth / _initialSheilds;
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
        _sheildHealth = Mathf.Min(_sheildHealth + _sheildRegenIterationAmount, _initialSheilds);
        float healthPercentage = (float)_sheildHealth / _initialSheilds;
        _mainCanvasScript.SetHealthPercentage(healthPercentage);

        if (_sheildHealth == _initialSheilds)
        {
            CancelInvoke();
        }
    }
}
