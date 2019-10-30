using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
public class JobStudy : MonoBehaviour
{
    [SerializeField] private bool useJobs;
    [SerializeField] private Transform pfZombie;
    private List<Zombie> zombieList;

    public class Zombie
    {
        public Transform transform;
        public float moveY;
    }
    private void Start()//Instantiating zombies at random positions and with random moveY with out actul movement
    {
        zombieList = new List<Zombie>();
        for (int i = 0; i < 1000; i++)
        {
            Transform zombieTransform = Instantiate(pfZombie, new Vector3(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f)), Quaternion.identity);
            zombieList.Add(new Zombie
            {
                transform = zombieTransform,
                moveY = UnityEngine.Random.Range(1f, 2f)
            });
        }
    }

    // Update is called once per frame    
    void Update()
    {
        float startTime = Time.realtimeSinceStartup;

        if (useJobs)
        {
            NativeArray<float3> positionArray = new NativeArray<float3>(zombieList.Count, Allocator.TempJob);
            NativeArray<float> moveYArray = new NativeArray<float>(zombieList.Count, Allocator.TempJob);

            for (int i = 0; i < zombieList.Count; i++)
            {
                positionArray[i] = zombieList[i].transform.position;
                moveYArray[i] = zombieList[i].moveY;
            }

            ReallyToughParallelJob reallyTougParallelJob = new ReallyToughParallelJob
            {
                deltaTime = Time.deltaTime,
                position = positionArray,
                moveY = moveYArray
            };
            JobHandle jobHandle = reallyTougParallelJob.Schedule(zombieList.Count, 100);
            jobHandle.Complete();

            for (int i = 0; i < zombieList.Count; i++)
            {
                zombieList[i].transform.position = positionArray[i];
                zombieList[i].moveY = moveYArray[i];
            }
            positionArray.Dispose();
            moveYArray.Dispose();
        }
        else
        {
            foreach (Zombie zombie in zombieList)
            {
                zombie.transform.position += new Vector3(0, zombie.moveY * Time.deltaTime);
                if (zombie.transform.position.y > 5f)
                {
                    zombie.moveY = -math.abs(zombie.moveY);
                }
                if (zombie.transform.position.y < -5f)
                {
                    zombie.moveY = +math.abs(zombie.moveY);
                }
                float value = 0f;
                for (int i = 0; i < 1000; i++)
                {
                    value = math.exp10(math.sqrt(value));
                }
            }
        }

        /* if (useJobs)
         {
             NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
             for (int i = 0; i < 10; i++)
             {
                 JobHandle jobHandle = ReallyToughTaskJob();
                 jobHandleList.Add(jobHandle);
             }
             JobHandle.CompleteAll(jobHandleList);
             jobHandleList.Dispose();
         }
         else
         {
             for (int i = 0; i < 10; i++)
             {
                 ReallyToughTask();
             }
         }*/
        Debug.Log(((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
    }

    private void ReallyToughTask()
    {
        float value = 0f;
        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
    private JobHandle ReallyToughTaskJob()
    {
        ReallyToughJob toughJob = new ReallyToughJob();
        return toughJob.Schedule();
    }
}
[BurstCompile]
public struct ReallyToughJob : IJob
{
    public void Execute()
    {
        float value = 0f;
        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}
public struct ReallyToughParallelJob : IJobParallelFor
{
    public NativeArray<float3> position;
    public NativeArray<float> moveY;
    public float deltaTime;
    public void Execute(int index)
    {
        position[index] += new float3(0, moveY[index] * deltaTime, 0);
        if (position[index].y > 5f)
        {
            moveY[index] = -math.abs(moveY[index]);
        }
        if (position[index].y < -5f)
        {
            moveY[index] = +math.abs(moveY[index]);
        }
        float value = 0f;
        for (int i = 0; i < 1000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}