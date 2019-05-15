using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace Tokamak.JobSystem
{
    public struct UpdateParticles : IJobParallelForTransform
    {
        public float moveSpeed;
        public int timestep;
        public int numTimesteps;
        public int numParticles;

        public void Execute(int index, TransformAccess transform)
        {
            Vector3 pos = transform.position;

            pos += Vector3.forward;

            transform.position = pos;
        }
    }
}