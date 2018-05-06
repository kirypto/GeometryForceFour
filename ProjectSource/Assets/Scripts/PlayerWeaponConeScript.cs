using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponConeScript : MonoBehaviour
{
    [SerializeField] private float _weaponForce = 1f;

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
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            FireGravityWeapon();
        }
    }

    private void FireGravityWeapon()
    {
        ISet<Collider2D> mobsWithinWeaponCone = new HashSet<Collider2D>();
        ContactFilter2D contactFilter2D = new ContactFilter2D
        {
                useLayerMask = true,
                layerMask = 1 << LayerMask.NameToLayer("Mobs")
        };

        Utilities.ScanForOverlappedColliders(_triangleCollider, contactFilter2D, mobsWithinWeaponCone);
        Utilities.ScanForOverlappedColliders(_ovalCollider, contactFilter2D, mobsWithinWeaponCone);

        foreach (Collider2D mobCollider in mobsWithinWeaponCone)
        {
            Vector2 directionToMob = ((Vector2)(mobCollider.transform.position - transform.position)).normalized;

            //TODO: Improve performance of this
            mobCollider.GetComponentInParent<Rigidbody2D>().AddForce(directionToMob * _weaponForce);
        }
    }

    private enum WeaponMode
    {
        // ReSharper disable once UnusedMember.Local
        None,
        GravPush,
        GravPull
    }
}
