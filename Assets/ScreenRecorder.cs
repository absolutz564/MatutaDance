using UnityEngine;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class ScreenRecorder : MonoBehaviour
{
    public Camera cameraToRecord;
    public int captureWidth = 1920;
    public int captureHeight = 1080;
    public int frameRate = 30;
    public float captureDuration = 5f;
    public string outputFileName = "output.mp4";

    private bool isRecording = false;
    private List<Texture2D> capturedFrames = new List<Texture2D>();

    public async void StartRecording()
    {
        UnityEngine.Debug.Log("Chamou Start Record");
        if (!isRecording)
        {
            await RecordScreenAsync();
        }
    }

    private async Task RecordScreenAsync()
    {
        UnityEngine.Debug.Log("recording");
        isRecording = true;
        capturedFrames.Clear();

        // Criar uma textura temporária para armazenar os frames capturados
        RenderTexture renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
        Texture2D texture = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
        cameraToRecord.targetTexture = renderTexture;

        // Iniciar a gravação
        float startTime = Time.time;
        while (Time.time - startTime < captureDuration)
        {
            await Task.Yield(); // Espera até o próximo frame

            // Capturar o frame atual da câmera
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
            texture.Apply();
            capturedFrames.Add(new Texture2D(texture.width, texture.height, texture.format, false));
            Graphics.CopyTexture(texture, capturedFrames[capturedFrames.Count - 1]);
        }

        // Limpar e desativar a textura de renderização
        cameraToRecord.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        // Verificar se os frames foram capturados corretamente
        if (capturedFrames.Count == 0)
        {
            UnityEngine.Debug.LogError("No frames were captured.");
            isRecording = false;
            return;
        }
        else
        {
            UnityEngine.Debug.Log($"Total frames captured: {capturedFrames.Count}");
        }

        // Converter os frames em um vídeo usando FFmpeg
        await FFMPEGConvertImagesToVideoAsync();
        isRecording = false;
    }

    private async Task FFMPEGConvertImagesToVideoAsync()
    {
        string tempDirectory = Path.Combine(Application.persistentDataPath, "TempFrames");
        string outputPath = Path.Combine(Application.persistentDataPath, outputFileName);
        string ffmpegPath = Path.Combine(Application.streamingAssetsPath, "FFmpegOut", "Windows", "ffmpeg.exe");

        try
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }
            else
            {
                foreach (var file in Directory.GetFiles(tempDirectory))
                {
                    File.Delete(file);
                }
            }

            // Salvar os frames capturados como arquivos PNG na pasta temporária
            for (int i = 0; i < capturedFrames.Count; i++)
            {
                string imageName = "frame_" + i.ToString("0000") + ".png";
                string imagePath = Path.Combine(tempDirectory, imageName);
                byte[] imageBytes = capturedFrames[i].EncodeToPNG();
                File.WriteAllBytes(imagePath, imageBytes);
            }

            // Configurar o comando FFmpeg para converter as imagens em um vídeo
            string imagePaths = $"-framerate {frameRate} -i \"{tempDirectory}/frame_%04d.png\"";
            string command = $"{imagePaths} -c:v libx264 -pix_fmt yuv420p \"{outputPath}\"";

            ProcessStartInfo processStartInfo = new ProcessStartInfo(ffmpegPath, command)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process ffmpegProcess = new Process())
            {
                ffmpegProcess.StartInfo = processStartInfo;
                ffmpegProcess.Start();

                string errorOutput = await ffmpegProcess.StandardError.ReadToEndAsync();
                UnityEngine.Debug.LogError("FFmpeg Error Output: " + errorOutput);

                bool outputFileCreated = false;
                await Task.Run(() =>
                {
                    ffmpegProcess.WaitForExit();
                    outputFileCreated = File.Exists(outputPath);
                });

                if (outputFileCreated)
                {
                    UnityEngine.Debug.Log("Video conversion finished. Output path: " + outputPath);
                }
                else
                {
                    UnityEngine.Debug.LogError("Output file not created.");
                }
            }

            // Excluir os frames temporários
            foreach (var file in Directory.GetFiles(tempDirectory, "frame_*.png"))
            {
                File.Delete(file);
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Error during video conversion: " + e.Message);
        }
    }
}
