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

using System.Net;
using System.Text;
using System.Threading.Tasks;
using PimDeWitte.UnityMainThreadDispatcher;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

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

    private HttpListener httpListener;
    private bool isRunning;
    public UnityMainThreadDispatcher dispacher;

    void Start()
    {
        StartServer();
    }
    //private string url = "https://usable-colt-reasonably.ngrok-free.app/";

    //IEnumerator CheckIfEndpointIsOk()
    //{
    //    using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
    //    {
    //        // Request and wait for the desired page.
    //        yield return webRequest.SendWebRequest();

    //        string[] pages = url.Split('/');
    //        int page = pages.Length - 1;

    //        if (webRequest.result != UnityWebRequest.Result.Success)
    //        {
    //            Debug.Log(pages[page] + ": Error: " + webRequest.error);
    //            InvokeRepeating("MakeGetRequest", 0f, 3f);
    //        }
    //        else
    //        {
    //            Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
    //            StartServer();
    //        }
    //    }
    //}
    private void StartServer()
    {

        //httpListener = new HttpListener();
        //httpListener.Prefixes.Add("http://*:8080/webhook/"); // Substitua pelo seu endpoint desejado
        //httpListener.Start();
        //isRunning = true;
        //UnityEngine.Debug.Log("Webhook server started...");

        //Task.Run(() => ListenForRequests());
    }

    private async Task ListenForRequests()
    {
        while (isRunning)
        {
            var context = await httpListener.GetContextAsync();
            ProcessRequest(context);
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        try
        {
            if (request.HttpMethod == "POST")
            {
                using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string requestBody = reader.ReadToEnd();

                    // Chame ProcessWebhook na thread principal
                    UnityMainThreadDispatcher.Instance().Enqueue(() => ProcessWebhook(requestBody));

                    // Prepara e envia a resposta
                    string responseString = "Webhook received";
                    byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;

                    using (var output = response.OutputStream)
                    {
                        output.Write(buffer, 0, buffer.Length);
                    }
                }
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            }
        }
        catch (Exception ex)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => UnityEngine.Debug.LogError("Error processing request: " + ex.Message));
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            string responseString = "Internal Server Error";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;

            using (var output = response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
            }
            InvokeRepeating("MakeGetRequest", 0f, 3f);
        }
        finally
        {
            // Certifique-se de fechar a resposta no bloco finally
            response.Close();
        }
    }

    public void MakeGetRequest()
    {
        PhotoTaker.Instance.MakeGetRequest();
    }

    private void ProcessWebhook(string requestBody)
    {
        // Esta chamada será executada na thread principal
        PhotoTaker.Instance.MakeGetRequest();

        // Implementar lógica de processamento do webhook
        UnityEngine.Debug.Log("Processing webhook: " + requestBody);

        // Exemplo: lógica específica com base no conteúdo do webhook
        if (requestBody.Contains("event_type_1"))
        {
            UnityEngine.Debug.Log("Processing event type 1");
            // Lógica específica para o evento tipo 1
        }
        else if (requestBody.Contains("event_type_2"))
        {
            UnityEngine.Debug.Log("Processing event type 2");
            // Lógica específica para o evento tipo 2
        }
        else
        {
            UnityEngine.Debug.Log("Unknown event type");
        }
    }

    private void OnDestroy()
    {
        isRunning = false;
        httpListener.Stop();
        httpListener.Close();
        UnityEngine.Debug.Log("Webhook server stopped.");
    }

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
            InteractionManager.instance.EffectsObject.SetActive(false);
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

            yield return new WaitForSeconds(1.0f);
        }

        UnityEngine.Debug.Log("Video upload locally complete");
    }


    public void StartCapture()
    {
        capturedFrames = new List<Texture2D>();


        StartCoroutine(CapturePhotosRoutine());
    }

    private IEnumerator CapturePhotosRoutine()
    {
        flashEffect.StartFlashing();
        //flashEffect.FlashEffectUpdateLoop();
        while (photoCount < numberOfPhotos)
        {
            yield return new WaitForSeconds(captureInterval);

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
