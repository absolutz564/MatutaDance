using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using System.Threading.Tasks;
using FfmpegUnity.Sample;
using FfmpegUnity;
using NekraliusDevelopmentStudio;
using UnityEngine.Video;

public class CapturePhotos : MonoBehaviour
{
    public GameObject BtnPreview;
    public GameObject Afterphoto;
    public RawImage RawVideoPlayer;
    public RawImage RawVideoPlayerPreview;
    public RawImage webcamRawImage; // Referência à RawImage que exibe o feed da webcam
    public int numberOfPhotos = 40; // Número de fotos a serem capturadas
    public float captureInterval; // Intervalo entre as capturas em segundos

    public List<Texture2D> capturedFrames; // Lista de frames capturados

    private int photoCount = 0;


    public float boomerangDuration = 1.5f; // Duração da aceleração no meio (segundos)

    public string outputName = "output.mp4";
    public int framerate = 40;

    private Process ffmpegProcess;
    string tempDirectory;
    string outputPath;
    string ffmpegPath;

    public VideoCreationFromTextureList videoCreationFromTextures;
    public FfmpegCaptureCommand ffmpegcapture;
    public PhotoTaker videoUploader;
    public GameObject VideoUploadMessage;
    public FlashEffect flashEffect;

    public int VideoDuration;
    public GameObject Moldura;
    public GameObject Anim;

    private void OnApplicationQuit()
    {
        if (ffmpegProcess != null && !ffmpegProcess.HasExited)
        {
            ffmpegProcess.Kill();
            ffmpegProcess.WaitForExit();

            if (File.Exists(outputPath))
            {
                // Delete the temporary image files
                foreach (var file in Directory.GetFiles(tempDirectory))
                {
                    File.Delete(file);
                }

                // Delete the temporary directory
                Directory.Delete(tempDirectory, true);
            }
        }
    }


    public void PlayCapturedFrames(float frameInterval)
    {
        if(!Afterphoto.activeSelf)
        {
            StartCoroutine(WaitStop());
            Moldura.SetActive(false);
            Anim.SetActive(false);
            UnityEngine.Debug.Log("Finished playing captured frames.");
            Afterphoto.SetActive(true);
        }
    }

    public RawImage rawImage;

    public VideoPlayer videoPlayer;

    void StartVideoPreview(string path)
    {
        videoPlayer.url = "file://" + path;
        videoPlayer.Play();
    }

    IEnumerator WaitStop()
    {

        yield return new WaitForSeconds(0.85f);

        ffmpegcapture.Stop();

        string videoFilePath = Path.Combine(Application.streamingAssetsPath, "ExportedVideos", "capture.mp4");

        // Aguarda um momento para garantir que o arquivo seja fechado
        yield return new WaitForSeconds(1.0f);
        InteractionManager.instance.audioSource.Stop();
        StartVideoPreview(videoFilePath);

        bool uploadSuccessful = false;

        while (!uploadSuccessful)
        {
            try
            {
                videoUploader.UploadVideo(videoFilePath);
                uploadSuccessful = true;
            }
            catch (IOException ex)
            {
                UnityEngine.Debug.LogWarning("Sharing violation, waiting and retrying: " + ex.Message);
            }

            yield return new WaitForSeconds(1.0f); // Aguarda 1 segundo antes de tentar novamente
        }

        UnityEngine.Debug.Log("Video upload locally complete");
    }


    public void StartCapture()
    {
        capturedFrames = new List<Texture2D>();

        // Iniciar a captura das fotos quando o script for ativado
        StartCoroutine(CapturePhotosRoutine());
    }

    private IEnumerator CapturePhotosRoutine()
    {
        flashEffect.StartFlashing();
        //flashEffect.FlashEffectUpdateLoop();
        while (photoCount < numberOfPhotos)
        {
            // Aguardar o intervalo de tempo definido
            yield return new WaitForSeconds(captureInterval);

            // Capturar a foto atual da RawImage da webcam e adicionar à lista
            CapturePhotoFromWebcam();

            photoCount++;
        }

        UnityEngine.Debug.Log("Captured all photos.");


        // Ativar o botão de visualização
        //BtnPreview.SetActive(true);
        //FFMPEGConvertImagesToVideo();
        //videoCreationFromTextures.InitFrames();
    }

    private void CapturePhotoFromWebcam()
    {
        if (webcamRawImage != null)
        {
            // Capturar o quadro atual da RawImage (assumindo que a webcam já está sendo exibida nela)
            Texture2D texture = new Texture2D(webcamRawImage.texture.width, webcamRawImage.texture.height, TextureFormat.RGBA32, false);
            RenderTexture previousActiveRenderTexture = RenderTexture.active;
            RenderTexture.active = webcamRawImage.texture as RenderTexture;
            texture.ReadPixels(new Rect(0, 0, webcamRawImage.texture.width, webcamRawImage.texture.height), 0, 0);
            texture.Apply();
            RenderTexture.active = previousActiveRenderTexture;

            //Texture2D sharpenedTexture = ImageProcessingUtils.ApplySharpenFilter(texture, 1.0f);

            // Adicionar a textura capturada e processada à lista
            capturedFrames.Add(texture);

        }
    }
}
