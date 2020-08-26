using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    [Header("SETTINGS:")]
    public GameObject Prefab;
    public float Radius;
    public float Mass;
    public float RestDensity;
    public float Viscosity;
    public float Drag;
    public int StepParticleInitialization = 1;

    [Header("SETUP:")]
    public int Amount = 100;
    public int ParticleInitiateHeight = -3200;

    private int countX = 1197;
    private int countZ = 1021;
    private int stepXZ = 50;
    private Particle[] particles;
    private ParticleCollider[] colliders;



    private void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void Initialize()

    {
        particles = new Particle[Amount];

        int stepPartInit = StepParticleInitialization * stepXZ;
        int particlePerSide = (int)Mathf.Round(Mathf.Sqrt(Amount));

        Debug.Log(particlePerSide);
        Debug.Log((float)particlePerSide/2);

        float startX = ((countX * stepXZ) / 2) - ((float)particlePerSide / 2) * stepPartInit;
        float startZ = ((countZ * stepXZ) / 2) - ((float)particlePerSide / 2) * stepPartInit;

        Debug.Log(startX);
        Debug.Log(startZ);

        for (int k = 0; k < particlePerSide; k++)
        {
            float z = startZ + (k * stepPartInit);

            for (int i = 0; i < particlePerSide; i++)
            {
                float x = startX + ((i % particlePerSide) * stepPartInit);
                float y = (float)ParticleInitiateHeight;

                GameObject currentGO = Instantiate(Prefab);
                Particle currentParticle = currentGO.AddComponent<Particle>();
                particles[k+i] = currentParticle;

                currentGO.transform.localScale = Vector3.one * Radius;
                currentGO.transform.position = new Vector3(x, y, z);

                currentParticle.Go = currentGO;
                currentParticle.Position = currentGO.transform.position;

            }
        }
    }
}
