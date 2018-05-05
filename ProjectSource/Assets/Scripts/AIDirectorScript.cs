using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class AIDirectorScript : MonoBehaviour
{
    [SerializeField] private int mobCount = 100;
    [SerializeField] private GameObject mobPrefab;
    [SerializeField] private bool CenterMassToggle = true;
    [SerializeField] private float StdCenterMassScaler = 1f;
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
    private Transform playerTransform;


    struct CenterMassJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<CenterMassJobInput> input;
        [ReadOnly] public NativeArray<MobComponentData> allMobs;
        [ReadOnly] public NativeArray<MobComponentData> friends;

        public NativeArray<CenterMassJobOutput> output;

        public void Execute(int index)
        {
            Vector3 center = Vector3.zero;

            for (int i = 0; i < 50; i++)
            {
                center += friends[i].Position;
            }

            center = (friends.Length > 1) ? center / (friends.Length) : center;

            output[index] = new CenterMassJobOutput()
            {
                GroupCenterMass = ((center - allMobs[index].Position) / 100f) *
                                  input[index].GroupCenterMassScale
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
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        allMobs = new GameObject[mobCount];
        allMobData = new NativeArray<MobComponentData>(mobCount, Allocator.Persistent);
        moveToPlayerOutput = new NativeArray<Vector3>(mobCount, Allocator.Persistent);
        centerMassOutputs = new NativeArray<CenterMassJobOutput>(mobCount, Allocator.Persistent);
        centerMassInputs = GetPrepopArray(new CenterMassJobInput()
        {
            GroupCenterMassScale = StdCenterMassScaler
        });


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
        Vector3 playerPos = playerTransform.position;


        var moveToPlayerJob = new MoveToPlayerJob()
        {
            allMobs = allMobData,
            playerPos = playerPos,
            output = moveToPlayerOutput
        };

        JobHandle jobHandle = moveToPlayerJob.Schedule(mobCount, 64);


        CenterMassJob centerMassJob = new CenterMassJob()
        {
            allMobs = allMobData,
            friends = allMobData,
            input = centerMassInputs,
            output = centerMassOutputs
        };
        JobHandle centerMassJobHandle = centerMassJob.Schedule(mobCount, 64);


        jobHandle.Complete();
        centerMassJobHandle.Complete();
        for (var i = 0; i < allMobs.Length; i++)
        {
            var data = allMobData[i];
            data.Velocity = Vector3.zero;
            data.Velocity += moveToPlayerOutput[i];
            data.Velocity += centerMassOutputs[i].GroupCenterMass;


            GameObject mob = allMobs[i];
            mob.GetComponent<Rigidbody2D>().AddForce(data.Velocity);
            data.Position = mob.transform.position;
            allMobData[i] = data;
        }
    }


    private void OnDestroy()
    {
        allMobData.Dispose();
        moveToPlayerOutput.Dispose();

        centerMassInputs.Dispose();
        centerMassOutputs.Dispose();
    }

    private NativeArray<T> GetPrepopArray<T>(T defVal) where T : struct
    {
        NativeArray<T> prepopArray = new NativeArray<T>(mobCount, Allocator.Persistent);
        for (int i = 0; i < prepopArray.Length; i++)
        {
            prepopArray[i] = defVal;
        }

        return prepopArray;
    }
}
