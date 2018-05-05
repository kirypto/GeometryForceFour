using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

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

    private NativeArray<Vector3> moveToPlayerOutput;


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

    struct MoveToPlayerJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<MobComponentData> allMobs;
        [ReadOnly] public Vector3 playerPos;

        public NativeArray<Vector3> output;

        public void Execute(int index)
        {
            output[index] = playerPos - allMobs[index].Position;
        }
    }


    private void Start()
    {
        allMobs = new GameObject[mobCount];
        allMobData = new NativeArray<MobComponentData>(mobCount, Allocator.Persistent);

        for (int i = 0; i < mobCount; i++)
        {
            var pos = new Vector3()
            {
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

    private void Update()
    {
        Vector3 zero = Vector3.zero;
        moveToPlayerOutput = new NativeArray<Vector3>(mobCount, Allocator.TempJob);


        var moveToPlayerJob = new MoveToPlayerJob()
        {
            allMobs = allMobData,
            playerPos = zero,
            output = moveToPlayerOutput
        };

        JobHandle jobHandle = moveToPlayerJob.Schedule(mobCount, 64);

        jobHandle.Complete();
        for (var i = 0; i < allMobs.Length; i++)
        {
            GameObject mob = allMobs[i];
            mob.GetComponent<Rigidbody2D>().AddForce(moveToPlayerOutput[i]);
            var data = allMobData[i];
            data.Position = mob.transform.position;
            allMobData[i] = data;
        }

        moveToPlayerOutput.Dispose();
    }


    private void OnDestroy()
    {
        allMobData.Dispose();
    }
}
