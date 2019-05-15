using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace Tokamak.JobSystem
{
    public class ManageParticles : MonoBehaviour
    {

        TransformAccessArray transforms;
        UpdateParticles updateJob;
        JobHandle updateHandle;

        private void OnDisable()
        {
            updateHandle.Complete();
            transforms.Dispose();
        }

        void Start()
        {
            transforms = new TransformAccessArray(0, -1);

            

        }

        void Update()
        {
            updateHandle.Complete();

            updateJob = new UpdateParticles()
            {
                moveSpeed = 1,
                timestep = 0,
                numTimesteps = 3000,
                numParticles = 1000
            };

            updateHandle = updateJob.Schedule(transforms);

            JobHandle.ScheduleBatchedJobs();
        }
    }
}
