using Nexweron.WebCamPlayer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;

namespace NekraliusDevelopmentStudio
{
    [System.Serializable]
    public class QRCodeData
    {
        public string qrcode;
        public string image;
    }

    [System.Serializable]
    public class Option
    {
        public string key;
        public string value;
    }

    [System.Serializable]
    public class Participant
    {
        public string id;
        public string name;
        public string last_name;
        public string phone;
        public bool agree_terms;
        public string experience_id;
        public List<Option> options;
        public bool status;
        public string current_event;
        public string mediaUrl;
        public string created_at;
        public string updated_at;
    }

    [System.Serializable]
    public class ParticipantDataWrapper
    {
        public List<Participant> participants;
        public int count;
    }

    public class PhotoTaker : MonoBehaviour
    {
        //Code made by Victor Paulo Melo da Silva - Game Developer - GitHub - https://github.com/Necralius
        //PhotoTaker - (0.1)
        //State: Functional - (Needs Refactoring, Needs Coments, Needs Improvement)
        //This code represents (Code functionality or code meaning)

        #region - Singleton Pattern -
        public static PhotoTaker Instance;
        private void Awake() => Instance = this;
        #endregion

        #region - Main Dependecies -
        [Header("System Dependencies")]
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private VideoPlayer videoPlayer2;
        public FlashEffect flashEffect;
        #endregion

        #region - Photo Show System
        [Header("Photo Shower")]
        public Sprite currentPhotoSprite;
        public GameObject photoShower;
        public Image photoReceiver;
        public GameObject objectsToDesapear;
        //public GameObject frameObject;
        public int waitTimeToShow = 4;
        #endregion

        #region - Countdown System -
        [Header("Countdown System")]
        public int timeToTakePhoto = 3;
        public TextMeshProUGUI countDownText;
        public SceneLoader sceneLoaderAsset;
        #endregion

        public GameObject photoTaker;
        public GameObject textCircle;

        #region - Photo Data -
        private Texture2D photoTexture;
        #endregion

        #region - DB Post -
        [Header("Photo Send DB")]
        public string currentPhotoLink;
        #endregion

        #region - Video Selector -
        [Header("Video Selection")]
        public VideoPos[] clips;
        public VideoClip currentClip;
        public VideoClip currentClip2;
        public GameObject videoRenderer;
        public GameObject videoRenderer2;

        public GameObject ArrowObject;
        public bool IsPhotoSolo = true;
        #endregion

        [Serializable]
        public class ResponseData
        {
            public string link;
        }

        public WebCamStream cameraStream;

        public GameObject Border;
        public RectTransform ObjectRect;
        public GameObject FirstImage;
        public GameObject SecondImage;
        public GameObject FfmpegObject;
        public CapturePhotos capturePhotos;

        public void SetVisibleImage(int index)
        {
            GameObject obj = index == 0 ? FirstImage : SecondImage;
            obj.SetActive(true);
        }
        private void Start()
        {
            currentClip = clips[0].clip;
            videoPlayer.clip = currentClip;
            currentClip2 = clips[0].clip2;
            videoPlayer2.clip = currentClip2;
            cameraStream.Play();
            Invoke("MakeGetRequest", 2f);

        }

        public void MakeGetRequest()
        {
            StartCoroutine(GetRequest());
        }
        public string EndPointParticipants;
        public Button InteractionStartButton;

        public string currentUserId;
        public string currentUserName;
        public string currentUserMatuta;
        public string currentUserMusic;

        //public TMP_InputField currentUserIdInput;
        public TMP_InputField currentUserNameInput;
        public TMP_InputField currentUserMatutaInput;
        public TMP_InputField currentUserMusicInput;
        public TextMeshProUGUI messageInput;
        public GameObject ModalParticipantInfo;
        IEnumerator GetRequest()
        {
            string currentEvent = "STARTED";
            int page = 1;
            int offset = 1;
            string order = "asc";
            string experience_id = "03f92bca-a5ee-44c7-b9e1-aaef0b4be4cd";

            string fullUrl = $"{url}{EndPointParticipants}?current_event={currentEvent}&experience_id={experience_id}&page={page}&offset={offset}&order={order}";

            using (UnityWebRequest request = UnityWebRequest.Get(fullUrl))
            {
                Debug.Log(fullUrl);
                request.SetRequestHeader("Authorization", "Bearer " + token);
                request.SetRequestHeader("access_token", accessToken);

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Erro na requisi��o: " + request.error);
                }
                else
                {
                    Debug.Log("Resposta da requisi��o: " + request.downloadHandler.text);
                    ParticipantDataWrapper participantDataWrapper = JsonUtility.FromJson<ParticipantDataWrapper>(request.downloadHandler.text);

                    if (participantDataWrapper.participants == null || participantDataWrapper.participants.Count == 0)
                    {
                        Debug.Log("Resposta vazia, n�o fazendo nada.");
                    }
                    else
                    {
                        CancelInvoke("MakeGetRequest");

                        if (!ModalParticipantInfo.activeSelf)
                        {
                            SceneManager.LoadScene("Screen2 - PhotoTaker");
                        }
                        else
                        {
                            Participant firstParticipant = participantDataWrapper.participants[0];
                            Debug.Log("ID: " + firstParticipant.id);
                            Debug.Log("Nome: " + firstParticipant.name);
                            currentUserId = firstParticipant.id;
                            currentUserName = firstParticipant.name;
                            messageInput.text = "Participante da vez";

                            foreach (var option in firstParticipant.options)
                            {
                                if (option.key == "Matuta Preferida")
                                {
                                    currentUserMatuta = option.value;
                                }
                                else if (option.key == "M�sica Preferida")
                                {
                                    currentUserMusic = option.value;
                                }
                                Debug.Log(option.key + ": " + option.value);
                            }

                            PopulateInputValues();
                            StartCoroutine(WaitToStartInteraction());
                        }
                    }
                }
            }
        }

        public void ReloadScene()
        {
            SceneManager.LoadScene("Screen2 - PhotoTaker");
        }


        IEnumerator WaitToStartInteraction()
        {
            yield return new WaitForSeconds(3);
            InteractionStartButton.onClick.Invoke();
        }

        public void PopulateInputValues()
        {
            currentUserNameInput.text = currentUserName;
            currentUserMusicInput.text = currentUserMusic;
            currentUserMatutaInput.text = currentUserMatuta;
        }

        public void StartPhotoTakeAction()
        {
            videoPlayer.Play();
            videoPlayer2.Play();

            cameraStream.Play();
            StartCoroutine(TakeScreenShot());
            if (!IsPhotoSolo)
            {
                StartCoroutine(WaitToShow());

            }
        }

        public void StartVideoAction()
        {
            cameraStream.Play();
            StartCoroutine(TakeVideo());
            //StartCoroutine(UpdateEvent(currentUserId));
        }

        //IEnumerator UpdateEvent(string userId)
        //{
        //    string url = $"{urlVideo}/agent/participants/{userId}/update-event";
        //    string payload = "{\"event\":\"STARTED\"}";

        //    Debug.Log("URL: " + url);
        //    Debug.Log("Payload: " + payload);

        //    UnityWebRequest request = new UnityWebRequest(url, "PATCH");
        //    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);
        //    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        //    request.downloadHandler = new DownloadHandlerBuffer();
        //    request.SetRequestHeader("Content-Type", "application/json");
        //    request.SetRequestHeader("Authorization", "Bearer " + token);
        //    request.SetRequestHeader("access_token", accessToken);

        //    yield return request.SendWebRequest();

        //    if (request.result != UnityWebRequest.Result.Success)
        //    {
        //        Debug.LogError("Erro na requisi��o: " + request.error);
        //        Debug.LogError("Status Code: " + request.responseCode);
        //    }
        //    else
        //    {
        //        Debug.Log("Resposta da requisi��o: " + request.downloadHandler.text);
        //    }
        //}

        IEnumerator TakeVideo()
        {
            countDownText.gameObject.SetActive(true);

            for (int i = timeToTakePhoto; i > 0; i--)
            {
                countDownText.gameObject.GetComponent<RescaleEffect>().StartEffect();
                countDownText.gameObject.GetComponent<RescaleEffect>().ResetScale();

                countDownText.text = i.ToString();
                if (i == 1)
                {
                    ArrowObject.SetActive(false);
                }
                yield return new WaitForSeconds(1);
            }
            countDownText.gameObject.SetActive(false);
            InteractionManager.instance.StartTimer();
            FfmpegObject.SetActive(true);
            capturePhotos.StartCapture();
            yield return new WaitForSeconds(capturePhotos.VideoDuration + 2);
            float frameRate = capturePhotos.framerate;
            float frameInterval = 1.0f / frameRate;

            // Iniciar a reprodu��o com a taxa de quadros desejada
            capturePhotos.PlayCapturedFrames(frameInterval);
            cameraStream.Stop();
        }

        IEnumerator WaitToShow()
        {
            Debug.Log("Exibindo renderes");
            yield return new WaitForSeconds(0.6f);
            videoRenderer.SetActive(true);
            videoRenderer2.SetActive(true);
        }
        public void SetTime(int currTime)
        {
            timeToTakePhoto = currTime;
        }
        IEnumerator TakeScreenShot()
        {
            capturePhotos.RawVideoPlayer.gameObject.SetActive(false);
            for (int i = timeToTakePhoto; i > 0; i--)
            {
                countDownText.gameObject.GetComponent<RescaleEffect>().StartEffect();
                countDownText.gameObject.GetComponent<RescaleEffect>().ResetScale();

                countDownText.text = i.ToString();
                if(i == 1)
                {
                    ArrowObject.SetActive(false);
                }
                yield return new WaitForSeconds(1);
            }
            textCircle.gameObject.SetActive(false);
            countDownText.gameObject.SetActive(false);

            yield return new WaitForEndOfFrame();
            int width = Screen.width;
            int height = Screen.height;

            yield return new WaitForEndOfFrame();

            int newWidth = width; 
            int newHeight = height;

            Texture2D screenShotTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0, 0, width, height);

            yield return new WaitForEndOfFrame();
            screenShotTexture.ReadPixels(rect, 0, 0);
            screenShotTexture.Apply();

            Texture2D resizedTexture = ScaleTexture(screenShotTexture, newWidth, newHeight);

            StartCoroutine(PhotoSend(resizedTexture));

            photoTexture = screenShotTexture;
            Border.SetActive(false);
            FirstImage.SetActive(false);
            SecondImage.SetActive(false);

            flashEffect.CallFlashEffect();

            ConvertPhoto(photoTexture);
            StartCoroutine(ShowPhoto());            
        }

        private Texture2D ScaleTexture(Texture2D source, int newWidth, int newHeight)
        {
            Texture2D resizedTexture = new Texture2D(newWidth, newHeight);
            Color[] pixels = new Color[newWidth * newHeight];

            float xRatio = (float)source.width / newWidth;
            float yRatio = (float)source.height / newHeight;


            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    int sourceX = Mathf.FloorToInt(x * xRatio);
                    int sourceY = Mathf.FloorToInt(y * yRatio);

                    pixels[y * newWidth + x] = source.GetPixel(sourceX, sourceY);
                }
            }

            resizedTexture.SetPixels(pixels);
            resizedTexture.Apply();

            return resizedTexture;
        }

        void ConvertPhoto(Texture2D textureData)
        {
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Rect rect = new Rect(0,0, textureData.width, textureData.height);
            currentPhotoSprite = Sprite.Create(textureData, rect, pivot);
            currentPhotoSprite.name = DateTime.Now.Ticks.ToString();

            photoReceiver.sprite = currentPhotoSprite;
        }
        IEnumerator ShowPhoto()
        {
            yield return new WaitForSeconds(waitTimeToShow);      

            objectsToDesapear.SetActive(false);
            photoShower.SetActive(true);
            sceneLoaderAsset.canLoad = true;
            cameraStream.Stop();
        }

        [SerializeField] public string url = "https://tmj-boticario.dilisgs.com.br/images/index.php";
        [SerializeField] public string endpoint = "myimage";
        [SerializeField] public string movedFolder = "/uploaded_images/";
        [SerializeField] public string id = "_image_.png";
        #region - Photo Sending to DB -
        private IEnumerator PhotoSend(Texture2D photo)
        {
            currentPhotoLink = "https://tmj-boticario.dilisgs.com.br/images/uploaded_images/image.png";
            WWWForm form = new WWWForm();
            byte[] textureBytes = null;
            //Texture2D imageTexture = GetTextureCopy(texture);
            textureBytes = photo.EncodeToPNG();
            DateTime currentDateTime = DateTime.Now;

            // Define o formato desejado (sem espa�os)
            string format = "yyyyMMddHHmmss";

            id = currentDateTime.ToString(format) + "_image_.png";

            form.AddBinaryData(endpoint, textureBytes, id, "image/png");

            using (WWW w = new WWW(url, form))
            {
                yield return w;

                if (!string.IsNullOrEmpty(w.error))
                {
                    Debug.Log("Error uploading image: " + w.error);
                    //WaitingMessage.SetActive(false);
                }
                else
                {
                    Debug.Log("Image uploaded successfully");
                    //WaitingMessage.SetActive(false);

                    //qrCodeTexture = texture;
                }

                string downloadURL = url + "?download=true&image=" + id;

                QR_CodeGenerator.Instance.finalLink = downloadURL;
                QR_CodeGenerator.Instance.isActive = true;
            }
        }


        [SerializeField] private string urlVideo = "http://145.14.134.34:3003";
        [SerializeField] private string endpointVideo = "/agent/participants/upload";
        [SerializeField] private string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InVuaXR5QG1hdHV0YS5jb20uYnIiLCJleHBlcmllbmNlX2lkIjoiMDNmOTJiY2EtYTVlZS00NGM3LWI5ZTEtYWFlZjBiNGJlNGNkIiwiaWF0IjoxNzE2NTE2MjQyLCJleHAiOjE3MjQyOTIyNDIsInN1YiI6ImFkYjBkZTM0LTdjOWUtNGY3Yi1iNDlmLTMzNWJkZTIzNDA4NCJ9.B__4UNl7MZmbEYmkIU0HGiQCQjbk_vyxSv6NmXVKg_E";
        [SerializeField] private string accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiREFOQ0FfTUFUVVRBIiwiaWF0IjoxNzE2NDk1MTgxLCJleHAiOjE3MjQyNzExODEsInN1YiI6IkRBTkNBX01BVFVUQSJ9.nmTQnXKxWVuEC8rBzHSAbEvulemfy5UFlCZN8zGDzE0";


        //[SerializeField] public string urlVideoHostgator = "https://festverao-saobraz.dilisgs.com.br/video-upload/index.php";
        //[SerializeField] public string endpointoHostgator = "myvideo";
        //[SerializeField] public string movedFolderVideo = "/uploaded_videos/";
        //[SerializeField] public string idVideo = "_video_.mp4";

        //private IEnumerator VideoSend(string videoFilePath)
        //{
        //    WWWForm form = new WWWForm();

        //    // Gerar um timestamp �nico para o nome do arquivo
        //    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

        //    // Adicionar o timestamp ao nome do arquivo
        //    string uniqueVideoFileName = "video_" + timestamp + ".mp4";

        //    // Carrega o arquivo de v�deo como bytes
        //    byte[] videoBytes = File.ReadAllBytes(videoFilePath);

        //    form.AddBinaryData(endpointoHostgator, videoBytes, uniqueVideoFileName, "video/mp4");

        //    using (WWW w = new WWW(urlVideoHostgator, form))
        //    {
        //        yield return w;

        //        if (!string.IsNullOrEmpty(w.error))
        //        {
        //            Debug.Log("Error uploading video: " + w.error);
        //            // Tratar o erro conforme necess�rio
        //            capturePhotos.VideoUploadMessage.SetActive(false);
        //        }
        //        else
        //        {
        //            Debug.Log("Video uploaded successfully");
        //            // Tratar o sucesso conforme necess�rio
        //            capturePhotos.VideoUploadMessage.SetActive(false);
        //        }

        //        string downloadURL = urlVideoHostgator + "?download=true&video=" + uniqueVideoFileName;

        //        QR_CodeGenerator.Instance.finalLink = downloadURL;
        //        QR_CodeGenerator.Instance.isActive = true;
        //    }
        //}

        public IEnumerator VideoSend(string videoFilePath)
        {
            Debug.Log("Starting video upload...");
            sendMessage.SetActive(true);

            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string uniqueVideoFileName = "video_" + timestamp + ".mp4";
            byte[] videoBytes = File.ReadAllBytes(videoFilePath);

            Debug.Log($"Loaded video file {videoFilePath}, size: {videoBytes.Length} bytes");

            WWWForm form = new WWWForm();
            form.AddField("isFileIdentify", "false");
            form.AddBinaryData("file", videoBytes, uniqueVideoFileName, "video/mp4");

            string fullUrl = urlVideo + endpointVideo;

            using (UnityWebRequest request = UnityWebRequest.Post(fullUrl, form))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
                request.SetRequestHeader("access_token", accessToken);


                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Error uploading video: {request.error}");
                    Debug.LogError($"Response Code: {request.responseCode}");
                    Debug.LogError($"Response: {request.downloadHandler.text}");

                }
                else
                {
                    Debug.Log("Video uploaded successfully");
                    Debug.Log(request);
                    Debug.Log(request.downloadHandler.text);
                    string jsonString = request.downloadHandler.text;
                    QRCodeData data = JsonUtility.FromJson<QRCodeData>(jsonString);

                    string base64Code = data.qrcode;

                    // Remove o prefixo "data:image/png;base64," do c�digo base64
                    string base64Only = base64Code.Substring(base64Code.IndexOf(",") + 1);

                    Debug.Log("Base64 Code: " + base64Only);

                    base64QRCode = base64Only;

                    StartCoroutine(LoadQRCode());

                }
                sendMessage.SetActive(false);
            }
        }

        public RawImage rawImage;
        public string base64QRCode;
        public GameObject sendMessage;

        private IEnumerator LoadQRCode()
        {
            // Decodifica a string base64 em uma textura
            byte[] bytes = Convert.FromBase64String(base64QRCode);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);

            // Define a textura na RawImage
            rawImage.texture = texture;
            sendMessage.SetActive(false);
            Invoke("ReloadScene", 120f);
            yield return null;
        }

        public void UploadVideo(string videoFilePath)
        {
            StartCoroutine(VideoSend(videoFilePath));
        }
        #endregion
        private string ValidateString(string textToValidade)
        {
            string link = "";
            for (int i = 0; i < textToValidade.Length; i++)
            {
                if (i > 8)
                {
                    if (i >= textToValidade.Length - 2) continue;
                    link += textToValidade.Substring(i, 1);
                }
            }
            return link;
        }

        #region - Unity Web Request Data Model - 
        private UnityWebRequest CreateRequest(string path, RequestType type = RequestType.GET, object data = null)
        {
            UnityWebRequest request = new UnityWebRequest(path, type.ToString());

            if (data != null)
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            return request;
        }

        [Serializable]
        public class PostData
        {
            public string image;
        }
        public enum RequestType
        {
            GET = 0,
            POST = 1,
            PUT = 2
        }
        #endregion

        #region - Video Clip Selection -
        public void SetCurrentClip(int videoIndex)
        {
            IsPhotoSolo = videoIndex < 0;
            if(videoIndex >= 0)
            {
                currentClip = clips[videoIndex].clip;
                //videoRenderer.transform.localPosition = clips[videoIndex].videoPosition;
                videoPlayer.clip = currentClip;

                currentClip2 = clips[videoIndex].clip2;
                ////videoRenderer2.transform.localPosition = clips[videoIndex].videoPosition;
                videoPlayer2.clip = currentClip2;
            }
            else
            {
                videoRenderer.SetActive(false);
                videoRenderer2.SetActive(false);
                currentClip = null;
                videoPlayer.clip = null;

                currentClip2 = null;
                videoPlayer2.clip = null;
            }
        }
        #endregion
    }

    [Serializable]
    public struct VideoPos
    {
        public Vector3 videoPosition;
        public VideoClip clip;
        public VideoClip clip2;
    }
}