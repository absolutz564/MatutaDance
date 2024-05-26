using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private void Awake()
    {
        instance = this;
    }
    public TextMeshProUGUI countdownText;
    public int initialTime = 30; // Tempo inicial em segundos
    private int currentTime; // Tempo atual em segundos
    public GameObject TimerObject;

    public void StartTimer()
    {
        TimerObject.SetActive(true);
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

    void UpdateCountdownText()
    {
        // Atualize o texto com o tempo atual formatado
        countdownText.text = currentTime.ToString();
    }
    public void StartInteraction(Animator itemAnim)
    {
        musicIndices.Add("Asa Branca", 1);
        musicIndices.Add("Forró Pesado", 2);
        musicIndices.Add("Olha a Fogueira", 3);

        PlayMusicByName(musicName);

        bottleIndices.Add("MATUTA UMBURANA", 1);
        bottleIndices.Add("MATUTA CRISTAL", 2);
        bottleIndices.Add("MATUTA MEL & LIMAO", 3);

        PlayMusicByName(musicName);
        ShowBotttle(bottleName);

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
                audioSource.clip = Musics[index];
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
        StartCoroutine(WaitToDance(itemAnim));
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
