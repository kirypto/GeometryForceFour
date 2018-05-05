using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class AIDirectorScript : MonoBehaviour
{
    [SerializeField] private int mobCount = 100;
    [SerializeField] private GameObject mobPrefab;
    [SerializeField] private bool CenterMassToggle = true;
    [SerializeField] private bool EqualizeSpeedToggle = true;
    [SerializeField] private bool PersonalSpaceToggle = true;


    private GameObject[] allMobs;
    private NativeArray<MobComponentData> allMobData;
    private NativeArray<CenterMassJobInput> centerMassInputs;
    private NativeArray<CenterMassJobOutput> centerMassOutputs;
    private NativeArray<EqualizeSpeedJobInput> equalizeSpeedInputs;
    private NativeArray<EqualizeSpeedJobOutput> equalizeSpeedOutputs;
    private NativeArray<PersonalSpaceJobInput> personalSpaceInputs;
    private NativeArray<PersonalSpaceJobOutput> personalSpaceOutputs;


//
//    struct FindFriendsJob : IJobParallelFor
//    {
//        public void Execute(int index)
//        {
//            throw new System.NotImplementedException();
//        }
//    }

    struct CenterMassJob : IJobParallelFor
    {
        [ReadOnly] private NativeArray<CenterMassJobInput> input;
        [ReadOnly] private NativeArray<MobComponentData> allMobs;
        [ReadOnly] private NativeArray<MobComponentData> friends;

        private NativeArray<CenterMassJobOutput> output;

        public void Execute(int index)
        {
            Vector3 center = Vector3.zero;

            for (int i = 0; index < friends.Length; i++)
            {
                if (!friends[i].Equals(allMobs[index]))
                {
                    center += friends[i].Position;
                }
            }

            center = (friends.Length > 1) ? center / (friends.Length - 1) : center;

            output[index] = new CenterMassJobOutput()
            {
                GroupCenterMass = (center - allMobs[index].Position) / 100f
            };
        }
    }


    private void Start()
    {
        allMobs = new GameObject[mobCount];
        allMobData = new NativeArray<MobComponentData>(mobCount, Allocator.Persistent);

        for (int i = 0; i < mobCount; i++)
        {
            var pos = new Vector3() {
                x = Random.Range(-8f, 8f),
                y = Random.Range(-5f, 5f)
            };

            var mob = Instantiate(mobPrefab, pos, Quaternion.identity);
            allMobs[i] = mob;
            var mobData = new MobComponentData()
            {
                Position = mob.transform.position,
                Velocity = Vector3.zero
            };
            allMobData[i] = mobData;
        }
    }

    private void OnDestroy()
    {
        allMobData.Dispose();
    }
}
