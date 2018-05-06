using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerScanMobScript : MonoBehaviour
{
    [SerializeField] private float _enemyScanIterationTime;
    [SerializeField] private int _damagePerHit;
    [SerializeField] private float _mobTeleportDistance;

    private Collider2D _mobScanCollider;
    private PlayerDamageScript _playerDamageScript;

    private void Awake()
    {
        _mobScanCollider = GetComponents<CircleCollider2D>().First(collider2D => collider2D.isTrigger);
        _playerDamageScript = GetComponent<PlayerDamageScript>();

        InvokeRepeating(nameof(ScanForMobs), _enemyScanIterationTime, _enemyScanIterationTime);
    }

    private void ScanForMobs()
    {
        ISet<Collider2D> mobsWithinMobScanCollider = new HashSet<Collider2D>();
        ContactFilter2D contactFilter2D = new ContactFilter2D
        {
                useLayerMask = true,
                layerMask = 1 << LayerMask.NameToLayer("Mobs")
        };

        Utilities.ScanForOverlappedColliders(_mobScanCollider, contactFilter2D, mobsWithinMobScanCollider);

        if (mobsWithinMobScanCollider.Count == 0)
        {
            return;
        }

        int playerDamage = _damagePerHit * mobsWithinMobScanCollider.Count;
        _playerDamageScript.TakeDamage(playerDamage);

        foreach (Collider2D mobCollider in mobsWithinMobScanCollider)
        {
            // TODO: Change this to something better
            mobCollider.transform.position += (Vector3)Random.insideUnitCircle.normalized * _mobTeleportDistance;
        }
    }
}
