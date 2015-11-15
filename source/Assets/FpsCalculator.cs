using System;

public class FpsCalculator
{
    float[] deltaTimes; // dt for last frames
    int sz = 0;

    public FpsCalculator(int numFrames)
    {
        deltaTimes = new float[numFrames];
    }

    public void Update(float dt)
    {
        if( sz < deltaTimes.Length )
            deltaTimes[sz++] = dt;
        else
        {
            // make space at the end
            for(int i = 1; i < sz; ++i)
                deltaTimes[i-1] = deltaTimes[i];
            deltaTimes[sz-1] = dt;
        }
    }

    public float fps
    {
        get
        {
            float avgDt = 0f;
            for(int i = 0; i < sz; ++i)
                avgDt += deltaTimes[i];

            avgDt /= sz;

            return 1 / avgDt;

        }
    }
}