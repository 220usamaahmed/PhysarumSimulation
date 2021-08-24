using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysarumSimulation : MonoBehaviour
{
    public ComputeShader PhysarumSimulationComputeShader;

    private const int WIDTH = 400;
    private const int HEIGHT = 400;

    private RenderTexture trailMap;

    private struct Agent
    {
        public Vector2 position;
        public float speed;
        public float angle;
    }

    private int NUM_AGENTS = 10000;
    private ComputeBuffer agentsBuffer;

    private void OnEnable()
    {
        List<Agent> agents = new List<Agent>();

        for (int i = 0; i < NUM_AGENTS; ++i)
        {
            Agent agent = new Agent();
            agent.position = new Vector2(WIDTH / 2, HEIGHT / 2);
            agent.speed = Random.Range(0.4f, 0.8f);
            agent.angle = Random.Range(0.0f, 2 * Mathf.PI);
            agents.Add(agent);
        }

        if (agentsBuffer != null) agentsBuffer.Release();

        if (agents.Count > 0)
        {
            agentsBuffer = new ComputeBuffer(agents.Count, 16);
            agentsBuffer.SetData(agents);
        }
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
