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
        ISet<Collider2D> collidersWithinWeaponCone = new HashSet<Collider2D>();
        ContactFilter2D contactFilter2D = new ContactFilter2D
        {
                useLayerMask = true,
                layerMask = 1 << LayerMask.NameToLayer("Mobs") | 1 << LayerMask.NameToLayer("Totem")
        };

        Utilities.ScanForOverlappedColliders(_triangleCollider, contactFilter2D, collidersWithinWeaponCone);
        Utilities.ScanForOverlappedColliders(_ovalCollider, contactFilter2D, collidersWithinWeaponCone);

        foreach (Collider2D weaponTargetCollider in collidersWithinWeaponCone)
        {
            Vector2 directionToMob = ((Vector2)(weaponTargetCollider.transform.position - transform.position)).normalized;

            //TODO: Improve performance of this
            Rigidbody2D rigidBodyToApplyForce = weaponTargetCollider.GetComponent<Rigidbody2D>();
            if (rigidBodyToApplyForce == null)
            {
                rigidBodyToApplyForce = weaponTargetCollider.GetComponentInParent<Rigidbody2D>();
            }

            rigidBodyToApplyForce.AddForce(directionToMob * _weaponForce);
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
