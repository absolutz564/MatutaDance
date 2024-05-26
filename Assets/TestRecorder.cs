using UnityEngine;
using System.Diagnostics;
using System.IO;

public class TestRecorder : MonoBehaviour
{
    public string ffmpegPath; // Caminho para o execut�vel do FFmpeg
    public string outputFilePath; // Caminho de sa�da para o v�deo
    public string outputName = "output.mp4";
    private void Start()
    {

        ffmpegPath = Path.Combine(Application.streamingAssetsPath, "FFmpegOut/Windows", "ffmpeg.exe");
        outputFilePath = Path.Combine(Application.streamingAssetsPath, "ExportedVideos", outputName);
        // Definir o tempo de grava��o em segundos
        float recordingTime = 5f;

        // Comando FFmpeg para gravar a gameview com �udio
        string command = "-f gdigrab -framerate 30 -i desktop -f dshow -i audio=\"virtual-audio-capturer\" -t " + recordingTime + " -c:v libx264 -preset ultrafast -qp 0 -c:a aac " + outputFilePath;

        // Iniciar o processo FFmpeg
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = ffmpegPath;
        startInfo.Arguments = command;
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();

        UnityEngine.Debug.Log("Grava��o conclu�da.");
    }
}
