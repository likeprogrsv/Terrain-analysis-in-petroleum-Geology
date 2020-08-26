/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleInitialization : MonoBehaviour
{
       
    public int stepParticalInitialization = 1;

    private void Initialize()

    {
        particles = new Particle[Amount];

        int stepPartInit = stepParticalInitialization * stepX;
        int particlePerSide = Math.Round(Math.Sqrt((double)Amount));

        float startX = ((CountX * stepX) / 2) - (particlePerSide / 2) * stepPartInit;
        float startZ = ((CountY * stepY) / 2) -(particlePerSide / 2) * stepPartInit;

        for (int i = 0; i < Amount; i++)

        {

            float x = startX + ((i % particlePerSide) * stepPartInit);
            float y = -3200f;
            float z = startZ + ((i % particlePerSide) * stepPartInit);

            
            GameObject currentGO = Instantiate(Prefab);
            Particle currentParticle = currentGO.AddComponent<Particle>();
            particles[i] = currentParticle;

            currentGO.transform.localScale = Vector3.one * Radius;
            currentGO.transform.position = new Vector3(x, y, z);
                        
            currentParticle.Go = currentGO;
            currentParticle.Position = currentGO.transform.position;

        }

    }

}
*/