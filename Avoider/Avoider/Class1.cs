﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System.Numerics;


namespace AVoider
{
    public class Class1
    {
        public static string GetGreeting()
        {
            return "Hello from MyPlugin";
        }
    }
    public class PoissonDiscSampler
    {
        private const int k = 30;  // Maximum number of attempts before marking a sample as inactive.

        private readonly Rect rect;
        private readonly float radius2;  // radius squared
        private readonly float cellSize;
        private Vector3[,] grid;
        private List<Vector3> activeSamples = new List<Vector3>();

        /// Create a sampler with the following parameters:
        ///
        /// width:  each sample's x coordinate will be between [0, width]
        /// height: each sample's y coordinate will be between [0, height]
        /// radius: each sample will be at least `radius` units away from any other sample, and at most 2 * `radius`.
        public PoissonDiscSampler(float width, float height, float radius)
        {
            rect = new Rect(0, 0, width, height);
            radius2 = radius * radius;
            cellSize = radius / Mathf.Sqrt(2);
            grid = new Vector3[Mathf.CeilToInt(width / cellSize),
                               Mathf.CeilToInt(height / cellSize)];
        }

        /// Return a lazy sequence of samples. You typically want to call this in a foreach loop, like so:
        ///   foreach (Vector2 sample in sampler.Samples()) { ... }
        public IEnumerable<Vector3> Samples()
        {
            // First sample is choosen randomly
            yield return AddSample(new Vector3(UnityEngine.Random.value * rect.width, UnityEngine.Random.value * rect.height));

            while (activeSamples.Count > 0)
            {

                // Pick a random active sample
                int i = (int)UnityEngine.Random.value * activeSamples.Count;
                Vector3 sample = activeSamples[i];

                // Try `k` random candidates between [radius, 2 * radius] from that sample.
                bool found = false;
                for (int j = 0; j < k; ++j)
                {

                    float angle = 2 * Mathf.PI * UnityEngine.Random.value;
                    float r = Mathf.Sqrt(UnityEngine.Random.value * 3 * radius2 + radius2); // See: http://stackoverflow.com/questions/9048095/create-random-number-within-an-annulus/9048443#9048443
                    Vector3 candidate = sample + r * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));

                    // Accept candidates if it's inside the rect and farther than 2 * radius to any existing sample.
                    if (rect.Contains(candidate) && IsFarEnough(candidate))
                    {
                        found = true;
                        yield return AddSample(candidate);
                        break;
                    }
                }

                // If we couldn't find a valid candidate after k attempts, remove this sample from the active samples queue
                if (!found)
                {
                    activeSamples[i] = activeSamples[activeSamples.Count - 1];
                    activeSamples.RemoveAt(activeSamples.Count - 1);
                }
            }
        }

        private bool IsFarEnough(Vector3 sample)
        {
            GridPos pos = new GridPos(sample, cellSize);

            int xmin = Mathf.Max(pos.x - 2, 0);
            int zmin = Mathf.Max(pos.z - 2, 0);
            int xmax = Mathf.Min(pos.x + 2, grid.GetLength(0) - 1);
            int zmax = Mathf.Min(pos.z + 2, grid.GetLength(1) - 1);

            for (int z = zmin; z <= zmax; z++)
            {
                for (int x = xmin; x <= xmax; x++)
                {
                    Vector3 s = grid[x, z];
                    if (s != Vector3.zero)
                    {
                        Vector3 d = s - sample;
                        if (d.x * d.x + d.z * d.z < radius2) return false;
                    }
                }
            }

            return true;

            // Note: we use the zero vector to denote an unfilled cell in the grid. This means that if we were
            // to randomly pick (0, 0) as a sample, it would be ignored for the purposes of proximity-testing
            // and we might end up with another sample too close from (0, 0). This is a very minor issue.
        }

        /// Adds the sample to the active samples queue and the grid before returning it
        private Vector3 AddSample(Vector3 sample)
        {
            activeSamples.Add(sample);
            GridPos pos = new GridPos(sample, cellSize);
            grid[pos.x, pos.z] = sample;
            return sample;
        }

        /// Helper struct to calculate the x and y indices of a sample in the grid
        private struct GridPos
        {
            public int x;
            public int z;

            public GridPos(Vector3 sample, float cellSize)
            {
                x = (int)(sample.x / cellSize);
                z = (int)(sample.z / cellSize);
            }
        }
    }
}
