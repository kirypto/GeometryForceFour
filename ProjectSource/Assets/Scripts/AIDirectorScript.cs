using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class AIDirectorScript : MonoBehaviour
{
    [SerializeField] private int mobCount = 100;
    [SerializeField] private GameObject mobPrefab;
    [SerializeField] private float maxMobSpeed = 8f;
    [SerializeField] private int jobSystemBatchSize = 64;
    [SerializeField] private float findFriendRadius = 5f;

    [SerializeField] private float moveToPlayerScaler = 1f;
    [SerializeField] public float targetPlayerInnerRadius = 5f;
    [SerializeField] public float targetPlayerOutterRadius = 30f;

    [SerializeField] private bool CenterMassToggle = true;
    [SerializeField] private float StdCenterMassScaler = 1f;

    [SerializeField] private bool EqualizeSpeedToggle = true;
    [SerializeField] private float equalizeSpeedRadius = 5f;
    [SerializeField] private float stdEqualizeSpeedScaler = 1f;

    [SerializeField] private bool PersonalSpaceToggle = true;
    [SerializeField] private float personalSpaceRadius = 0.5f;
    [SerializeField] private float stdPersonalSpaceScaler = 1f;

    [SerializeField] private int _updateFriendBatchSize = 5;
    [SerializeField] private float _updateFriendsCoroutineIterationTime = 0.1f;


    private GameObject[] allMobs;
    private NativeArray<MobComponentData> allMobData;
    private NativeMultiHashMap<int, MobComponentData> mobFriends;
    private NativeArray<CenterMassJobOutput> centerMassOutputs;
    private NativeArray<EqualizeSpeedJobOutput> equalizeSpeedOutputs;
    private NativeArray<PersonalSpaceJobOutput> personalSpaceOutputs;

    private NativeArray<Vector3> moveToPlayerOutput;
    private Transform playerTransform;

    private IDictionary<int, IList<MobComponentData>> _friendDictionary;
    private JobHandle _moveToPlayerJobHandle;
    private JobHandle _centerMassJobHandle;
    private JobHandle _personalSpaceJobHandle;
    private JobHandle _equalizeSpeedJobHandle;

    struct FindFriendsJob : IJob
    {
        [ReadOnly] public float radius;
        [ReadOnly] public NativeArray<MobComponentData> allMobs;

        public NativeMultiHashMap<int, MobComponentData> output;

        public void Execute()
        {
            output.Clear();
            for (int index = 0; index < allMobs.Length; index++)
            {
                for (int i = 0; i < allMobs.Length; i++)
                {
                    float dist = Vector3.Distance(allMobs[index].Position, allMobs[i].Position);
                    if (dist < radius)
                    {
                        output.Add(index, allMobs[i]);
                    }
                }
            }
        }
    }

//    struct FindFriendsJob : IJobParallelFor
//    {
//        [ReadOnly] public float radius;
//        [ReadOnly] public NativeArray<MobComponentData> allMobs;
//
//        public NativeMultiHashMap<int, MobComponentData> output;
//        
//        public void Execute(int index)
//        {
//            output.Remove(index);
//            for (int i = 0; i < allMobs.Length; i++)
//            {
//                float dist = Vector3.Distance(allMobs[index].Position, allMobs[i].Position);
//                if (dist < radius)
//                {
//                    output.Add(index, allMobs[i]);
//                }
//            }   
//        }
//    }


    struct CenterMassJob : IJobParallelFor
    {
        [ReadOnly] public float scaler;
        [ReadOnly] public NativeArray<MobComponentData> allMobs;
        [ReadOnly] public NativeMultiHashMap<int, MobComponentData> friends;

        public NativeArray<CenterMassJobOutput> output;

        public void Execute(int index)
        {
            Vector3 center = Vector3.zero;

            int friendCount = 0;
            MobComponentData currentFrienData;
            NativeMultiHashMapIterator<int> frienderator = new NativeMultiHashMapIterator<int>();
            bool success = friends.TryGetFirstValue(index, out currentFrienData, out frienderator);

            while (success)
            {
                friendCount++;
                center += currentFrienData.Position;
                success = friends.TryGetNextValue(out currentFrienData, ref frienderator);
            }

            center = (friendCount > 1) ? center / (friendCount) : center;

            output[index] = new CenterMassJobOutput()
            {
                GroupCenterMass = ((center - allMobs[index].Position) / 100f) *
                                  scaler
            };
        }
    }

    struct PersonalSpaceJob : IJobParallelFor
    {
        [ReadOnly] public float scaler;
        [ReadOnly] public float radius;
        [ReadOnly] public NativeArray<MobComponentData> allMobs;
        [ReadOnly] public NativeMultiHashMap<int, MobComponentData> friends;

        public NativeArray<PersonalSpaceJobOutput> output;

        public void Execute(int index)
        {
            Vector3 avoidance = Vector3.zero;

            MobComponentData currentFrienData;
            NativeMultiHashMapIterator<int> frienderator = new NativeMultiHashMapIterator<int>();
            bool success = friends.TryGetFirstValue(index, out currentFrienData, out frienderator);

            while (success)
            {
                float dist = Vector3.Distance(allMobs[index].Position, currentFrienData.Position);

                if (dist > 0 && dist < radius)
                {
                    Vector3 diff = Vector3.Normalize(allMobs[index].Position - currentFrienData.Position);
                    diff = diff / dist;
                    avoidance += diff;
                }

                success = friends.TryGetNextValue(out currentFrienData, ref frienderator);
            }


            output[index] = new PersonalSpaceJobOutput()
            {
                PersonalSpace = avoidance * scaler
            };
        }
    }

    struct EqualizeSpeedJob : IJobParallelFor
    {
        [ReadOnly] public float scaler;
        [ReadOnly] public float radius;
        [ReadOnly] public NativeArray<MobComponentData> allMobs;
        [ReadOnly] public NativeMultiHashMap<int, MobComponentData> friends;

        public NativeArray<EqualizeSpeedJobOutput> output;

        public void Execute(int index)
        {
            Vector3 perceivedVelocity = Vector3.zero;

            int friendCount = 0;
            MobComponentData currentFrienData;
            NativeMultiHashMapIterator<int> frienderator = new NativeMultiHashMapIterator<int>();
            bool success = friends.TryGetFirstValue(index, out currentFrienData, out frienderator);

            while (success)
            {
                friendCount++;
                float dist = Vector3.Distance(allMobs[index].Position, currentFrienData.Position);

                if (dist < radius)
                {
                    perceivedVelocity += currentFrienData.Velocity;
                }

                success = friends.TryGetNextValue(out currentFrienData, ref frienderator);
            }

            perceivedVelocity = (friendCount > 1) ? perceivedVelocity / (friendCount) : perceivedVelocity;

            output[index] = new EqualizeSpeedJobOutput()
            {
                EqualizeSpeed = ((perceivedVelocity - allMobs[index].Velocity) / 8f) * scaler
            };
        }
    }

    struct MoveToPlayerJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<MobComponentData> allMobs;
        [ReadOnly] public Vector3 playerPos;
        [ReadOnly] public float scaler;
        [ReadOnly] public float targetPlayerInnerRadius;
        [ReadOnly] public float targetPlayerOutterRadius;

        public NativeArray<Vector3> output;

        public void Execute(int index)
        {
            float dist = Vector3.Distance(playerPos, allMobs[index].Position);
            if (dist < targetPlayerInnerRadius || dist > targetPlayerOutterRadius)
            {
                output[index] = (playerPos - allMobs[index].Position) * scaler;
            }
        }
    }


    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        allMobs = new GameObject[mobCount];
        allMobData = new NativeArray<MobComponentData>(mobCount, Allocator.Persistent);

        moveToPlayerOutput = new NativeArray<Vector3>(mobCount, Allocator.Persistent);

        centerMassOutputs = new NativeArray<CenterMassJobOutput>(mobCount, Allocator.Persistent);

        personalSpaceOutputs = new NativeArray<PersonalSpaceJobOutput>(mobCount, Allocator.Persistent);

        equalizeSpeedOutputs = new NativeArray<EqualizeSpeedJobOutput>(mobCount, Allocator.Persistent);

        mobFriends = new NativeMultiHashMap<int, MobComponentData>(mobCount, Allocator.Persistent);

        SpawnMobs();

        _friendDictionary = new Dictionary<int, IList<MobComponentData>>();
        for (int mobIndex = 0; mobIndex < mobCount; mobIndex++)
        {
            _friendDictionary[mobIndex] = new List<MobComponentData>();
            UpdateFriends(mobIndex);
        }

        StartCoroutine(nameof(UpdateFriendsCoroutine));
    }

    private void SpawnMobs()
    {
        for (int i = 0; i < mobCount; i++)
        {
            var pos = new Vector3()
            {
                x = Random.Range(-35f, 35f),
                y = Random.Range(-35f, 35f)
            };

            var mob = Instantiate(mobPrefab, pos, Quaternion.identity);
            if (i < 50)
            {
                mob.GetComponent<AudioSource>().Play();
            }

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
//        FindFriendsJob findFriendsJob = new FindFriendsJob()
//        {
//            allMobs = allMobData,
//            radius = findFriendRadius,
//            output = mobFriends
//        };
//        JobHandle findFriendsJobHandle = findFriendsJob.Schedule();
//        mobFriends.Clear();
//        for (int index = 0; index < mobCount; index++)
//        {
//            foreach (MobComponentData mobComponentData in _friendDictionary[index])
//            {
//                mobFriends.Add(index, mobComponentData);
//            }
//        }
        if (!_moveToPlayerJobHandle.IsCompleted || !_centerMassJobHandle.IsCompleted ||
            !_personalSpaceJobHandle.IsCompleted ||
            !_equalizeSpeedJobHandle.IsCompleted)
        {
            return;
        }

        _moveToPlayerJobHandle.Complete();
        _centerMassJobHandle.Complete();
        _personalSpaceJobHandle.Complete();
        _equalizeSpeedJobHandle.Complete();
        for (var i = 0; i < allMobs.Length; i++)
        {
            var data = allMobData[i];
            data.Velocity = Vector3.zero;
            data.Velocity += moveToPlayerOutput[i];
            data.Velocity += centerMassOutputs[i].GroupCenterMass;
            data.Velocity += personalSpaceOutputs[i].PersonalSpace;
            data.Velocity += equalizeSpeedOutputs[i].EqualizeSpeed;

            data.Velocity = Vector3.ClampMagnitude(data.Velocity, maxMobSpeed);


            GameObject mob = allMobs[i];
            mob.GetComponent<Rigidbody2D>().AddForce(data.Velocity);
            mob.transform.right = data.Velocity.normalized;
            data.Position = mob.transform.position;
            allMobData[i] = data;
        }


        Vector3 playerPos = playerTransform.position;

        var moveToPlayerJob = new MoveToPlayerJob()
        {
            allMobs = allMobData,
            playerPos = playerPos,
            targetPlayerInnerRadius = targetPlayerInnerRadius,
            targetPlayerOutterRadius = targetPlayerOutterRadius,
            scaler = moveToPlayerScaler,
            output = moveToPlayerOutput
        };

        _moveToPlayerJobHandle = moveToPlayerJob.Schedule(mobCount, jobSystemBatchSize);


        CenterMassJob centerMassJob = new CenterMassJob()
        {
            allMobs = allMobData,
            friends = mobFriends,
            scaler = StdCenterMassScaler,
            output = centerMassOutputs
        };
        _centerMassJobHandle = centerMassJob.Schedule(mobCount, jobSystemBatchSize);
//        JobHandle centerMassJobHandle = centerMassJob.Schedule(mobCount, jobSystemBatchSize, findFriendsJobHandle);


        PersonalSpaceJob personalSpaceJob = new PersonalSpaceJob()
        {
            allMobs = allMobData,
            friends = mobFriends,
            scaler = stdPersonalSpaceScaler,
            radius = personalSpaceRadius,
            output = personalSpaceOutputs
        };
        _personalSpaceJobHandle = personalSpaceJob.Schedule(mobCount, jobSystemBatchSize);
//        JobHandle personalSpaceJobHandle = personalSpaceJob.Schedule(mobCount, jobSystemBatchSize, findFriendsJobHandle);


        EqualizeSpeedJob equalizeSpeedJob = new EqualizeSpeedJob()
        {
            allMobs = allMobData,
            friends = mobFriends,
            scaler = stdEqualizeSpeedScaler,
            radius = equalizeSpeedRadius,
            output = equalizeSpeedOutputs
        };
        _equalizeSpeedJobHandle = equalizeSpeedJob.Schedule(mobCount, jobSystemBatchSize);
//        JobHandle equalizeSpeedJobHandle = equalizeSpeedJob.Schedule(mobCount, jobSystemBatchSize, findFriendsJobHandle);

//        moveToPlayerJobHandle.Complete();
//        centerMassJobHandle.Complete();
//        personalSpaceJobHandle.Complete();
//        equalizeSpeedJobHandle.Complete();
    }


    private void OnDestroy()
    {
        _moveToPlayerJobHandle.Complete();
        _centerMassJobHandle.Complete();
        _personalSpaceJobHandle.Complete();
        _equalizeSpeedJobHandle.Complete();
        allMobData.Dispose();
        moveToPlayerOutput.Dispose();

        centerMassOutputs.Dispose();

        personalSpaceOutputs.Dispose();

        equalizeSpeedOutputs.Dispose();

        mobFriends.Dispose();
    }

//    private NativeArray<T> GetPrepopArray<T>(T defVal) where T : struct
//    {
//        NativeArray<T> prepopArray = new NativeArray<T>(mobCount, Allocator.Persistent);
//        for (int i = 0; i < prepopArray.Length; i++)
//        {
//            prepopArray[i] = defVal;
//        }

//        return prepopArray;
//    }

    private IEnumerator UpdateFriendsCoroutine()
    {
        int numberUpdated = 0;
        while (true)
        {
            for (int currentMobIndex = 0; currentMobIndex < mobCount; currentMobIndex++)
            {
//                while (!_centerMassJobHandle.IsCompleted)
//                {
//                    print($"Center mass job not done, waiting: {_centerMassJobHandle.IsCompleted}");
//                    yield return new WaitWhile(() => _centerMassJobHandle.IsCompleted);
//                    print($"Should be done now: {_centerMassJobHandle.IsCompleted}");
//                }
//                print($"Updating Friends, center mass status: {_centerMassJobHandle.IsCompleted}");
//                _moveToPlayerJobHandle.Complete();

//                yield return new WaitUntil(() =>
//                {
//                    print($"Waiting: {_centerMassJobHandle.IsCompleted}, {_personalSpaceJobHandle.IsCompleted}, " +
//                          $" {_equalizeSpeedJobHandle.IsCompleted}");
//                    return _centerMassJobHandle.IsCompleted
//                           && _personalSpaceJobHandle.IsCompleted
//                           && _equalizeSpeedJobHandle.IsCompleted;
//                });
//                print("Updating...");
                _centerMassJobHandle.Complete();
                _personalSpaceJobHandle.Complete();
                _equalizeSpeedJobHandle.Complete();
                UpdateFriends(currentMobIndex);

                if (numberUpdated >= _updateFriendBatchSize)
                {
                    numberUpdated = 0;
//                    yield return new WaitForSeconds(1f);
                    yield return new WaitForSeconds(_updateFriendsCoroutineIterationTime);
//                    yield return new WaitForEndOfFrame();
                } else
                {
                    numberUpdated++;
                }
            }
        }

        // ReSharper disable once IteratorNeverReturns
    }

    private void UpdateFriends(int currentMobIndex)
    {
//        _centerMassJobHandle.Complete();
        mobFriends.Remove(currentMobIndex);
//        _friendDictionary[currentMobIndex].Clear();
        for (int otherMobIndex = 0; otherMobIndex < mobCount; otherMobIndex++)
        {
            float dist = Vector2.Distance(allMobData[currentMobIndex].Position, allMobData[otherMobIndex].Position);
            if (dist < findFriendRadius)
            {
                mobFriends.Add(currentMobIndex, allMobData[otherMobIndex]);
//                _friendDictionary[currentMobIndex].Add(allMobData[otherMobIndex]);
            }
        }

//        print($"Updated mob friends for: {currentMobIndex}");
    }

//    mobFriends.Clear();
//    for (int index = 0; index < mobCount; index++)
//    {
//        foreach (MobComponentData mobComponentData in _friendDictionary[index])
//        {
//            mobFriends.Add(index, mobComponentData);
//        }
//    }
}
