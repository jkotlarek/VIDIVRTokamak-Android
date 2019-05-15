using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class LoadParticles : MonoBehaviour
{
    public Transform tokamak;
    public List<Particle> particles;
    public int numParticle;
    public int numTimestep;
    public string file;
    public bool runOnce = false;
    public bool isReady = false;

    Task read;
    Task scale;
    Task convert;
    Task classify;
    string fullPath;
    bool printInfo = true;
    float scaleFactor;


    void Start()
    {
        scaleFactor = tokamak.localScale.x;
    }


    void Update()
    {
        if (runOnce)
        {
            DoLoadParticles();
        }
    }


    public async void DoLoadParticles()
    {
        fullPath = Application.dataPath + "/StreamingAssets/" + file + ".dat";

        particles = new List<Particle>();

        read = Task.Run(() => Read(fullPath));
        await read;
    }


    public void Read(string path)
    {
        Debug.Log("Reading: " + path);
        StreamReader sr = new StreamReader(path);
        char[] delimiter = new char[] { ' ' };

        DateTime start = DateTime.Now;
        int pID = 0;

        //Read until EOF
        while (sr.Peek() > 0)
        {
            int tStep = 0;
            string[] line = sr.ReadLine().Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

            //New Particle
            if (line.Length == 1)
            {
                pID = int.Parse(line[0]);
                particles.Add(new Particle(pID));
                tStep = 0;
            }
            //Timestep for particle
            else if (line.Length == 3)
            {
                particles[pID].timesteps[tStep++] = new Vector3(
                    float.Parse(line[0]),
                    float.Parse(line[1]),
                    float.Parse(line[2]));
            }
        }

        if (printInfo) Debug.Log("ReadParticleData.Read: " + (DateTime.Now - start).ToString());
    }


    public void Transform(float scaleR, float transR, float scaleZ, float transZ)
    {
        DateTime start = DateTime.Now;
        foreach (Particle particle in particles)
        {
            for (int i = 0; i < particle.timesteps.Length; i++)
            {
                Vector3 timestep = particle.timesteps[i];
                timestep.x = timestep.x * scaleR + transR;
                timestep.y = timestep.y * scaleZ + transZ;
                particle.timesteps[i] = timestep;
            }
        }
        if (printInfo) Debug.Log("ReadParticleData.Transform: " + (DateTime.Now - start).ToString());
    }


    public void Classify()
    {
        DateTime start = DateTime.Now;
        foreach (Particle p in particles)
        {
            List<int> inflectionPtIndex = new List<int>();
            List<bool> inflectionPtValue = new List<bool>();
            float prevDelta = 0f;
            float distance = 0f;
            for (int i = 0; i < p.timesteps.Length; i++)
            {
                float prevTheta = p.timesteps[i - 1].z;
                float theta = p.timesteps[i].z;
                float delta = theta - prevTheta;
                if (i > 1)
                {
                    //if different signs OR if this is last iteration
                    if ((delta < 0 && prevDelta > 0) || (delta > 0 && prevDelta < 0) || (i == p.timesteps.Length - 1))
                    {
                        //if particle has moved enough since last inflection point, it is passing: (false), otherwise it is trapped: (true)
                        inflectionPtValue.Add(distance < 3.14f);
                        inflectionPtIndex.Add(i);
                        distance = 0;
                    }

                    distance += Mathf.Abs(delta);
                }


                prevDelta = delta;
            }

            int inflectEnd = 0;
            int j = 0;
            bool inflectVal = true;

            for (int i = 0; i < p.timesteps.Length; i++)
            {
                if (inflectEnd == i && i != p.timesteps.Length - 1)
                {
                    inflectEnd = inflectionPtIndex[j];
                    inflectVal = inflectionPtValue[j];
                    j++;

                }
                p.trapped[i] = inflectVal;
            }
        }

        if (printInfo) Debug.Log("ReadParticleData.Classify: " + (DateTime.Now - start).ToString());
    }


    public void ToRectangular(bool flat = true)
    {
        DateTime start = DateTime.Now;

        foreach (Particle particle in particles)
        {
            for (int i = 0; i < particle.timesteps.Length; i++)
            {
                //timeStep.x: r * cos(phi)
                //timeStep.y: z
                //timeStep.z: r * sin(phi) 

                Vector3 timestep = particle.timesteps[i];
                timestep.x = timestep.x * Mathf.Cos(timestep.z);
                timestep.z = timestep.x * Mathf.Sin(timestep.z);
                particle.timesteps[i] = timestep;
            }
        }
        if (printInfo) Debug.Log("ReadParticleData.ToRectangular: " + (DateTime.Now - start).ToString());
    }
}


public class Particle
{
    const int numTimestep = 3000;

    public int index;
    public Vector3[] timesteps;
    public bool[] trapped;

    public Particle(int index)
    {
        this.index = index;
        timesteps = new Vector3[numTimestep];
        trapped = new bool[numTimestep];
    }

    public Particle(int index, Vector3[] timesteps, bool[] trapped)
    {
        this.index = index;
        this.timesteps = timesteps;
        this.trapped = trapped;
    }
}
