using UnityEngine;

public class PlayerWeaponConeScript : MonoBehaviour
{
    private Transform _weaponCone;
    private Transform _weaponConeTriangleComponent;
    private Transform _weaponConeOvalComponent;

    private void Awake()
    {
        _weaponCone = transform.Find("WeaponCone");
        _weaponConeTriangleComponent = _weaponCone.Find("TriangleComponent");
        _weaponConeOvalComponent = _weaponCone.Find("OvalComponent");
    }

    private void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        Vector2 vectorToMouse = ((Vector2)(mousePosition - transform.position)).normalized;
        transform.right = vectorToMouse;
    }
}
