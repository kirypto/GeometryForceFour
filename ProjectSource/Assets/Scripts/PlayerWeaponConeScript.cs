using UnityEngine;

public class PlayerWeaponConeScript : MonoBehaviour
{
    private Transform _weaponCone;
    private Transform _triangleTransform;
    private Transform _ovalTransform;

    private Collider2D _triangleCollider;
    private Collider2D _ovalCollider;


    private void Awake()
    {
        _weaponCone = transform.Find("WeaponCone");
        _triangleTransform = _weaponCone.Find("TriangleComponent");
        _ovalTransform = _weaponCone.Find("OvalComponent");

        _triangleCollider = _triangleTransform.GetComponent<PolygonCollider2D>();
        _ovalCollider = _ovalTransform.GetComponent<CapsuleCollider2D>();

        _triangleCollider.enabled = false;
        _ovalCollider.enabled = false;
    }


}
