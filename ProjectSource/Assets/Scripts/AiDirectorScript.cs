using System.Dynamic;
using Unity.Collections;
using UnityEngine;

public class AiDirectorScript : MonoBehaviour
{

    [SerializeField] private bool CohesionToggle = true;
    [SerializeField] private bool PersonalSpaceToggle = true;
    [SerializeField] private bool GroupCenterMassToggle = true;

    private NativeArray<MobComponentData> allMobs;

    struct MobComponentData
    {
        public Vector3 Velocity;

        // match speed
        public Vector3 Cohesion;
        public float CohesionScale;
        public float CohesionRadius;

        // personal space
        public Vector3 Avoidance;
        public float AvoidanceScale;
        public float AvoidanceRadius;

        // target center of group
        public Vector3 GroupCenterMass;
        public float GroupCenterMassScale;
    } 

	
}
