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

        print(_weaponCone);
        print(_weaponConeTriangleComponent);
        print(_weaponConeOvalComponent);
    }
}
