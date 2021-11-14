using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysarumSimulation : MonoBehaviour
{
    public ComputeShader PhysarumSimulationComputeShader;

    private const int WIDTH = 1080;
    private const int HEIGHT = 1080;

    private RenderTexture trailMap;

    private struct Agent
    {
        public Vector2 position;
        public float speed;
        public float angle;
        public int species;
    }

    private int NUM_AGENTS = Mathf.CeilToInt(WIDTH * HEIGHT * 0.2f);
    private ComputeBuffer agentsBuffer;

    private void OnEnable()
    {
        List<Agent> agents = InitAgents01();

        if (agentsBuffer != null) agentsBuffer.Release();

        if (agents.Count > 0)
        {
            agentsBuffer = new ComputeBuffer(agents.Count, 20);
            agentsBuffer.SetData(agents);
        }
    }

    private List<Agent> InitAgents01()
    {
        List<Agent> agents = new List<Agent>();

        float theta;
        float r = 240;

        for (int i = 0; i < NUM_AGENTS; ++i)
        {
            Agent agent = new Agent();
            
            theta = Random.Range(0, 2 * Mathf.PI);
            agent.position = new Vector2(
                WIDTH * 0.5f + r * Mathf.Cos(theta), 
                HEIGHT * 0.5f + r * Mathf.Sin(theta));
            agent.speed = Random.Range(0.4f, 0.8f);
            agent.angle = theta;
            agent.species = Random.Range(0, 3);
            agents.Add(agent);
        }

        return agents;
    }

    private List<Agent> InitAgents02()
    {
        List<Agent> agents = new List<Agent>();

        for (int i = 0; i < NUM_AGENTS / 20; ++i)
        {
            for (int j = 0; j < 10; ++j)
            {
                Agent agent = new Agent();
                agent.position = new Vector2(j * WIDTH / 10, 0);
                agent.speed = Random.Range(0.2f, 0.8f);
                agent.angle = Mathf.PI / 2;
                agent.species = 0;
                agents.Add(agent);
            }

            for (int j = 0; j < 10; ++j)
            {
                Agent agent = new Agent();
                agent.position = new Vector2(0, j * WIDTH / 10);
                agent.speed = Random.Range(0.2f, 0.3f);
                agent.angle = 0;
                agent.species = 1;
                agents.Add(agent);
            }
        }

        return agents;
    }

    private List<Agent> InitAgents03()
    {
        List<Agent> agents = new List<Agent>();

        for (int i = 0; i < NUM_AGENTS; ++i)
        {
            Agent agent = new Agent();
            agent.position = new Vector2(Random.Range(0, WIDTH), HEIGHT / 2);
            agent.speed = Random.Range(0.6f, 1.2f);
            agent.angle = Random.Range(0, Mathf.PI * 2);
            agent.species = Random.Range(0, 3);
            agents.Add(agent);
        }

        return agents;
    }

    private void OnDisable()
    {
        if (agentsBuffer != null)
            agentsBuffer.Release();
    }

    void Start()
    {
        PhysarumSimulationComputeShader.SetInt("canvasWidth", WIDTH);
        PhysarumSimulationComputeShader.SetInt("canvasHeight", HEIGHT);
        PhysarumSimulationComputeShader.SetInt("numAgents", NUM_AGENTS);
    }

    private static void InitRenderTexture(ref RenderTexture renderTexture)
    {
        if (renderTexture == null || renderTexture.width != WIDTH || renderTexture.height != HEIGHT)
        {
            if (renderTexture != null) renderTexture.Release();

            renderTexture = new RenderTexture(
                WIDTH, 
                HEIGHT, 
                0, 
                RenderTextureFormat.ARGBFloat,
                RenderTextureReadWrite.Linear);
            renderTexture.enableRandomWrite = true;
            renderTexture.useMipMap = false;
            renderTexture.Create();
        }
    }

    private void Render(RenderTexture dest) {
        InitRenderTexture(ref trailMap);

        PhysarumSimulationComputeShader.SetTexture(0, "UpdateAgentTrailMap", trailMap);
        if (agentsBuffer != null) PhysarumSimulationComputeShader.SetBuffer(0, "Agents", agentsBuffer);
        PhysarumSimulationComputeShader.Dispatch(0, Mathf.CeilToInt(NUM_AGENTS / 16.0f), 1, 1);

        PhysarumSimulationComputeShader.SetTexture(1, "DiffuseAndDissipateTrailMap", trailMap);
        PhysarumSimulationComputeShader.Dispatch(1, Mathf.CeilToInt(WIDTH / 8.0f), Mathf.CeilToInt(HEIGHT / 8.0f), 1);

        Graphics.Blit(trailMap, dest);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Render(dest);
    }
}
