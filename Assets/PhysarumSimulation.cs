using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysarumSimulation : MonoBehaviour
{
    public ComputeShader PhysarumSimulationComputeShader;

    private const int WIDTH = 400;
    private const int HEIGHT = 400;

    private RenderTexture testRenderTexture;

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
        InitRenderTexture(ref testRenderTexture);

        PhysarumSimulationComputeShader.SetTexture(0, "Result", testRenderTexture);

        PhysarumSimulationComputeShader.Dispatch(0, WIDTH / 8, HEIGHT / 8, 1);

        Graphics.Blit(testRenderTexture, dest);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Render(dest);
    }
}
