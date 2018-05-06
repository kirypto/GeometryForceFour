using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

public class AIDirectorScript : MonoBehaviour
{
    [SerializeField] private int mobCount = 100;
    [SerializeField] private GameObject mobPrefab;

    [SerializeField] private float moveToPlayerScaler = 1f;

    [SerializeField] private bool CenterMassToggle = true;
    [SerializeField] private float StdCenterMassScaler = 1f;

    [SerializeField] private bool EqualizeSpeedToggle = true;
    [SerializeField] private float equalizeSpeedRadius = 5f;
    [SerializeField] private float stdEqualizeSpeedScaler = 1f;

    [SerializeField] private bool PersonalSpaceToggle = true;
    [SerializeField] private float personalSpaceRadius = 0.5f;
    [SerializeField] private float stdPersonalSpaceScaler = 1f;


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

            for (int i = 0; i < friends.Length; i++)
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

    struct PersonalSpaceJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<PersonalSpaceJobInput> input;
        [ReadOnly] public NativeArray<MobComponentData> allMobs;
        [ReadOnly] public NativeArray<MobComponentData> friends;

        public NativeArray<PersonalSpaceJobOutput> output;

        public void Execute(int index)
        {
            Vector3 avoidance = Vector3.zero;

            for (int i = 0; i < friends.Length; i++)
            {
                float dist = Vector3.Distance(allMobs[index].Position, friends[i].Position);

                if (dist > 0 && dist < input[index].PersonalSpaceRadius)
                {
                    Vector3 diff = Vector3.Normalize(allMobs[index].Position - friends[i].Position);
                    diff = diff / dist;
                    avoidance += diff;
                }
            }


            output[index] = new PersonalSpaceJobOutput()
            {
                PersonalSpace = avoidance * input[index].PersonalSpaceScale
            };
        }
    }

    struct EqualizeSpeedJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<EqualizeSpeedJobInput> input;
        [ReadOnly] public NativeArray<MobComponentData> allMobs;
        [ReadOnly] public NativeArray<MobComponentData> friends;

        public NativeArray<EqualizeSpeedJobOutput> output;

        public void Execute(int index)
        {
            Vector3 perceivedVelocity = Vector3.zero;

            for (int i = 0; i < friends.Length; i++)
            {
                float dist = Vector3.Distance(allMobs[index].Position, friends[i].Position);

                if (dist < input[index].EqualizeSpeedRadius)
                {
                    perceivedVelocity += friends[i].Velocity;
                }
            }

            perceivedVelocity = (friends.Length > 1) ? perceivedVelocity / (friends.Length) : perceivedVelocity;

            output[index] = new EqualizeSpeedJobOutput()
            {
                EqualizeSpeed = ((perceivedVelocity - allMobs[index].Velocity) / 8f) * input[index].EqualizeSpeedScale
            };
        }
    }

    struct MoveToPlayerJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<MobComponentData> allMobs;
        [ReadOnly] public Vector3 playerPos;
        [ReadOnly] public float scaler;

        public NativeArray<Vector3> output;

        public void Execute(int index)
        {
            output[index] = (playerPos - allMobs[index].Position) * scaler;
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

        personalSpaceOutputs = new NativeArray<PersonalSpaceJobOutput>(mobCount, Allocator.Persistent);
        personalSpaceInputs = GetPrepopArray(new PersonalSpaceJobInput()
        {
            PersonalSpaceRadius = personalSpaceRadius,
            PersonalSpaceScale = stdPersonalSpaceScaler
        });

        equalizeSpeedOutputs = new NativeArray<EqualizeSpeedJobOutput>(mobCount, Allocator.Persistent);
        equalizeSpeedInputs = GetPrepopArray(new EqualizeSpeedJobInput()
        {
            EqualizeSpeedRadius = equalizeSpeedRadius,
            EqualizeSpeedScale = stdEqualizeSpeedScaler
        });

        SpawnMobs();
    }

    private void SpawnMobs()
    {
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
            scaler = moveToPlayerScaler,
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


        PersonalSpaceJob personalSpaceJob = new PersonalSpaceJob()
        {
            allMobs = allMobData,
            friends = allMobData,
            input = personalSpaceInputs,
            output = personalSpaceOutputs
        };
        JobHandle personalSpaceJobHandle = personalSpaceJob.Schedule(mobCount, 64);


        EqualizeSpeedJob equalizeSpeedJob = new EqualizeSpeedJob()
        {
            allMobs = allMobData,
            friends = allMobData,
            input = equalizeSpeedInputs,
            output = equalizeSpeedOutputs
        };
        JobHandle equalizeSpeedJobHandle = equalizeSpeedJob.Schedule(mobCount, 64);


        jobHandle.Complete();
        centerMassJobHandle.Complete();
        personalSpaceJobHandle.Complete();
        equalizeSpeedJobHandle.Complete();
        for (var i = 0; i < allMobs.Length; i++)
        {
            var data = allMobData[i];
            data.Velocity = Vector3.zero;
            data.Velocity += moveToPlayerOutput[i];
            data.Velocity += centerMassOutputs[i].GroupCenterMass;
            data.Velocity += personalSpaceOutputs[i].PersonalSpace;
            data.Velocity += equalizeSpeedOutputs[i].EqualizeSpeed;


            GameObject mob = allMobs[i];
            mob.GetComponent<Rigidbody2D>().AddForce(data.Velocity);
            mob.transform.right = data.Velocity.normalized;
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

        personalSpaceInputs.Dispose();
        personalSpaceOutputs.Dispose();

        equalizeSpeedInputs.Dispose();
        equalizeSpeedOutputs.Dispose();
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
