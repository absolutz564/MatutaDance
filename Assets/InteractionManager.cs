using NekraliusDevelopmentStudio;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class InteractionManager : MonoBehaviour
{
    public float minWaitTimeStart = 4.0f;
    public float maxWaitTimeStart = 6.0f;
    public float minWaitTimeJudge = 1.0f;
    public float maxWaitTimeJudge = 1.0f;
    public float minWaitTimeDance = 4f;
    public float maxWaitTimeDance = 5f;

    public float randomizedTimeStart;
    public float randomizedTimeJudge;
    public float randomizedTimeDance;

    public Animator animVideo;
    public Animator animPreview;

    public List<AudioClip> Musics;
    public AudioSource audioSource;
    public string musicName;

    public List<Sprite> BottleSprites;
    public Image BottleImage;
    public string bottleName;

    private Dictionary<string, int> musicIndices = new Dictionary<string, int>();


    private Dictionary<string, int> bottleIndices = new Dictionary<string, int>();

    public static InteractionManager instance;
    public GameObject EffectsObject;
    private void Awake()
    {
        instance = this;
    }
    public TextMeshProUGUI countdownText;
    public int initialTime = 30; // Tempo inicial em segundos
    private int currentTime; // Tempo atual em segundos
    public GameObject TimerObject;

    public int musicIndex;
    public UnityEngine.UI.Slider slider;

    public void LoadMusic()
    {

    }

    public void LoadImageBottle()
    {

    }
    public CapturePhotos capturePhotos;
    public void StartTimer()
    {
        TimerObject.SetActive(true);
        capturePhotos.Moldura.SetActive(true);
        currentTime = initialTime;

        // Atualize o texto inicial
        UpdateCountdownText();

        // Inicie a contagem regressiva
        StartCountdown();
    }

    void StartCountdown()
    {
        // Inicie uma rotina para decrementar o tempo a cada segundo
        StartCoroutine(CountdownRoutine());
        StartCoroutine(AnimateSlider());
    }

    IEnumerator CountdownRoutine()
    {
        while (currentTime > 0)
        {
            yield return new WaitForSeconds(1f); // Aguarde 1 segundo

            // Decrementar o tempo
            currentTime--;

            // Atualizar o texto
            UpdateCountdownText();
        }

        // Quando o tempo acabar, atualize o texto para "Fim do Tempo!"
        countdownText.text = "Fim do Tempo!";
    }
    private IEnumerator AnimateSlider()
    {
        float currentTime = 0f;

        // Enquanto o tempo atual for menor que a duração, atualize o valor do Slider
        while (currentTime < 20)
        {
            float sliderValue = Mathf.Lerp(1f, 0f, currentTime / 20);
            slider.value = sliderValue;

            currentTime += Time.deltaTime;
            yield return null; // Espera o próximo frame
        }

        slider.value = 0f;
    }

    void UpdateCountdownText()
    {
        // Atualize o texto com o tempo atual formatado
        countdownText.text = currentTime.ToString();
    }

    private HashSet<int> dance1MusicValues = new HashSet<int> { 1, 5, 8, 9, 42, 18, 10, 7, 29, 30, 31, 32, 33 };
    private HashSet<int> dance2MusicValues = new HashSet<int> { 6, 11, 12, 23, 21, 35, 34, 36, 25, 39, 44 };
    private HashSet<int> dance3MusicValues = new HashSet<int> { 2, 3, 22, 20, 19, 24, 41, 43, 40 };
    private HashSet<int> dance4MusicValues = new HashSet<int> { 4, 13, 14, 15, 16, 17, 37, 38, 26, 28, 27 };

    public void StartInteraction(Animator itemAnim)
    {
        musicIndices.Add("Tem café - Henry Freitas", 1);
        musicIndices.Add("Casca de bala - Thullio Milionário", 2);
        musicIndices.Add("Pega o Guanabara - Wesley Safadão", 3);
        musicIndices.Add("Maravilhosa - Zé Vaqueiro", 4);
        musicIndices.Add("Dano Sarrada - Japãozin & Marina Sena", 5);
        musicIndices.Add("Uber - Xand Avião", 6);
        musicIndices.Add("Vaqueira - Eric Land", 7);
        musicIndices.Add("Novinha Bandida - Henry Freitas", 8);
        musicIndices.Add("É o Henry - Henry Freitas", 9);
        musicIndices.Add("Toca o Trompete - Felipe Amorim", 10);
        musicIndices.Add("Love Gostozinho - Nattan", 11);
        musicIndices.Add("Barulhin do Prazer - Henry Freitas", 12);
        musicIndices.Add("São João na Terra - Mastruz com Leite", 13);
        musicIndices.Add("Explode coração - Mastruz com Leite", 14);
        musicIndices.Add("Olhinhos de Fogueira - Mastruz com Leite", 15);
        musicIndices.Add("Só o Filé - Mastruz com Leite", 16);
        musicIndices.Add("Olha pro Céu - Mastruz com Leite", 17);
        musicIndices.Add("Verdadeiro amor - Magníficos", 18);
        musicIndices.Add("Cristal quebrado - Magníficos", 19);
        musicIndices.Add("Chupa que é de uva - Aviões do Forró", 20);
        musicIndices.Add("Toma conta de mim - Limão com Mel", 21);
        musicIndices.Add("É chamego ou xaveco - Magníficos", 22);
        musicIndices.Add("Diga sim pra mim - Desejo de Menina", 23);
        musicIndices.Add("Frevo Mulher - Trio Virgulino", 24);
        musicIndices.Add("Oh Chuva - Falamansa", 25);
        musicIndices.Add("Forro de tamanco - Os 3 dos Nordeste", 26);
        musicIndices.Add("É proibido cochilar - Os 3 dos Nordeste", 27);
        musicIndices.Add("Forró do Xenhenhém - Elba Ramalho", 28);
        musicIndices.Add("Espumas ao vento - Flávio José", 29);
        musicIndices.Add("Destá - Dorgival Dantas", 30);
        musicIndices.Add("Meu cenário - Petrucio Amorin", 31);
        musicIndices.Add("Me diz amor - Flávio José", 32);
        musicIndices.Add("Diga sim pra mim - Desejo de Menina2", 33);
        musicIndices.Add("Xote dos milagres - Falamansa", 34);
        musicIndices.Add("Rindo À Toa - Falamansa", 35);
        musicIndices.Add("Colo de menina - Rastapé", 36);
        musicIndices.Add("Isso aqui tá bom demais - Dominguinhos", 37);
        musicIndices.Add("Pagode Russo - Luiz Gonzaga", 38);
        musicIndices.Add("Filho do Mato - Rai Saia Rodada", 39);
        musicIndices.Add("Vou virar fazendeiro - Rai Saia Rodada", 40);
        musicIndices.Add("Carinha de Neném - Japãozin", 41);
        musicIndices.Add("Solinho do Brabo - Japãozin", 42);
        musicIndices.Add("Liga o paredão - Fabiano Guimarães", 43);
        musicIndices.Add("Respeita o interior - Fabiano Guimarães", 44);
        musicIndices.Add("Respeita o interior - Fabiano Guimarãess", 45);

        bottleIndices.Add("Matuta Umburana", 1);
        bottleIndices.Add("Matuta Cristal", 2);
        bottleIndices.Add("Abelha Rainha", 3);
        bottleIndices.Add("Matuta Bálsamo", 4);
        bottleIndices.Add("Matuta single Blend's", 5);
        bottleIndices.Add("Matuta Black Blend's", 6);

        PlayMusicByName(PhotoTaker.Instance.currentUserMusic);
        //PlayMusicByName(PhotoTaker.Instance.currentUserMusicInput.text);
        Debug.Log("Deu play na musica " + PhotoTaker.Instance.currentUserMusic);
        ShowBotttle(PhotoTaker.Instance.currentUserMatuta);

        randomizedTimeStart = Random.Range(minWaitTimeStart, maxWaitTimeStart);
        randomizedTimeJudge = Random.Range(minWaitTimeJudge, maxWaitTimeJudge);
        randomizedTimeDance = Random.Range(minWaitTimeDance, maxWaitTimeDance);
        StartCoroutine(ActivateObjectAfterRandomTime(itemAnim));
    }



    private void PlayMusicByName(string name)
    {
        // Verifica se o nome existe no dicionário
        if (musicIndices.TryGetValue(name, out int index))
        {
            // Verifica se o índice está dentro do intervalo da lista
            if (index >= 0 && index < Musics.Count)
            {
                // Define o clip de áudio e toca a música
                audioSource.clip = Musics[index-1];
                musicIndex = index - 1;
                audioSource.Play();
            }
            else
            {
                Debug.LogError("Índice fora do intervalo da lista de músicas.");
            }
        }
        else
        {
            Debug.LogError("Nome da música não encontrado.");
        }
    }



    private void ShowBotttle(string name)
    {
        if (bottleIndices.TryGetValue(name, out int index))
        {
            if (index >= 0 && index < BottleSprites.Count)
            {
                BottleImage.sprite = BottleSprites[index];
            }
            else
            {
                Debug.LogError("Índice fora do intervalo da lista de imagens.");
            }
        }
        else
        {
            Debug.LogError("Nome da imagem não encontrado.");
        }
    }

    private IEnumerator ActivateObjectAfterRandomTime(Animator itemAnim)
    {
        // Gera um valor aleatório entre minWaitTime e maxWaitTime

        // Aguarda o tempo gerado
        yield return new WaitForSeconds(randomizedTimeStart);

        // Ativa o GameObject
        itemAnim.gameObject.SetActive(true);
        StartCoroutine(WaitToJudge(itemAnim));
    }

    private IEnumerator WaitToJudge(Animator itemAnim)
    {
        // Aguarda o tempo gerado
        yield return new WaitForSeconds(randomizedTimeJudge);

        // Array com os nomes dos triggers
        string[] triggers = { "julgando1", "julgando2" };

        // Escolhe um índice aleatório entre 0 e 1
        int randomIndex = Random.Range(0, triggers.Length);

        // Define o trigger usando o nome aleatório escolhido
        itemAnim.SetTrigger(triggers[randomIndex]);
        //StartCoroutine(WaitToDance(itemAnim));
    }

    public void StartAnimEffects()
    {
        EffectsObject.SetActive(true);
    }

    public void StartDance()
    {
        string[] triggers = { "dance1", "dance2", "dance3" };

        int currentMusicValue = musicIndex + 1;
        Debug.Log(currentMusicValue);
        if (dance1MusicValues.Contains(currentMusicValue))
        {
            animVideo.SetTrigger("dance1");
        }
        else if (dance2MusicValues.Contains(currentMusicValue))
        {
            animVideo.SetTrigger("dance2");
        }
        else if (dance3MusicValues.Contains(currentMusicValue))
        {
            animVideo.SetTrigger("dance3");
        }
        else if (dance4MusicValues.Contains(currentMusicValue))
        {
            animVideo.SetTrigger("dance4");
        }
        else
        {
            animVideo.SetTrigger("dance1");
        }
        //int randomIndex = Random.Range(0, triggers.Length);

        //// Define o trigger usando o nome aleatório escolhido
        //animVideo.SetTrigger(triggers[randomIndex]);
    }
    private IEnumerator WaitToDance(Animator itemAnim)
    {
        // Aguarda o tempo gerado
        yield return new WaitForSeconds(randomizedTimeDance);

        // Array com os nomes dos triggers
        string[] triggers = { "dance1", "dance2", "dance3" };

        // Escolhe um índice aleatório entre 0 e 1
        int randomIndex = Random.Range(0, triggers.Length);

        // Define o trigger usando o nome aleatório escolhido
        itemAnim.SetTrigger(triggers[randomIndex]);
    }
}
