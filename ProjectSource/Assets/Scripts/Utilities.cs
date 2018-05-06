using System.Collections.Generic;
using UnityEngine;

public static class Utilities {
    public static void ScanForOverlappedColliders(Collider2D colliderToScan,
            ContactFilter2D contactFilter2D,
            ISet<Collider2D> setToAddFoundCollidersTo,
            int maxNumberOfCollidersToScan = 100)
    {
        Collider2D[] overlappingColliderResultArray = new Collider2D[maxNumberOfCollidersToScan];
        int numberOfOverlapColliders = colliderToScan.OverlapCollider(contactFilter2D, overlappingColliderResultArray);
        for (int i = 0; i < numberOfOverlapColliders; i++)
        {
            setToAddFoundCollidersTo.Add(overlappingColliderResultArray[i]);
        }
    }
}
