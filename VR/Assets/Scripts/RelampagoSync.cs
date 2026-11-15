using System.Collections;
using UnityEngine;
using UnityEngine.Video;

// Dispara raio (partículas), flash de luz e trovão nos momentos em que o vídeo da skybox mostra relâmpagos.
// Coloque em um objeto vazio da cena (ex.: "Tempestade") e preencha os campos no Inspector.
public class RelampagoSync : MonoBehaviour
{
    [Header("Vídeo da skybox")]
    [SerializeField] private VideoPlayer videoPlayer;
    [Tooltip("Segundos do vídeo em que cada relâmpago aparece (anote olhando o vídeo)")]
    [SerializeField] private float[] temposDosRelampagos;
    [Tooltip("(Opcional) Direção do raio em graus (0-360) para cada relâmpago, na mesma ordem dos tempos. Vazio = aleatório")]
    [SerializeField] private float[] angulosEmGraus;

    [Header("Raio e luz")]
    [SerializeField] private ParticleSystem raios;   // Particle System do raio (emissão em 0, Play On Awake desligado)
    [SerializeField] private Transform jogador;      // o raio aparece ao redor dele
    [SerializeField] private float distanciaDoRaio = 60f;
    [SerializeField] private float alturaDoRaio = 40f;
    [SerializeField] private Light luzRelampago;     // Directional Light com intensidade 0, que pisca nas montanhas/água
    [SerializeField] private float intensidadeMaxima = 3f;
    [Tooltip("Ligado: toca o efeito inteiro (use para efeitos prontos com vários Particle Systems dentro). Desligado: emite só 1 partícula (sistema simples)")]
    [SerializeField] private bool tocarEfeitoCompleto = true;
    [Tooltip("Quanto tempo o efeito fica tocando antes de ser parado (segundos)")]
    [SerializeField] private float duracaoDoEfeito = 0.6f;

    [Header("Teste")]
    [SerializeField] private bool testarAgora; // no Play Mode, marque esta caixinha para disparar um relâmpago de teste

    [Header("Trovão")]
    [SerializeField] private AudioSource audioTrovao;
    [SerializeField] private AudioClip[] sonsDeTrovao;
    [Tooltip("Atraso entre o relâmpago e o trovão (min, max), em segundos")]
    [SerializeField] private Vector2 atrasoDoTrovao = new Vector2(0.3f, 1.5f);

    private double tempoAnterior = -0.01;
    private Coroutine piscando;

    void Start()
    {
        if (luzRelampago != null) luzRelampago.intensity = 0f;

        // efeitos prontos costumam começar tocando sozinhos: garante que comecem parados
        if (raios != null) raios.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // avisos para ajudar a achar o que está faltando no Inspector
        if (videoPlayer == null) Debug.LogWarning("RelampagoSync: falta o VideoPlayer da skybox.");
        if (temposDosRelampagos == null || temposDosRelampagos.Length == 0) Debug.LogWarning("RelampagoSync: a lista 'Tempos Dos Relampagos' está vazia.");
        if (raios == null) Debug.LogWarning("RelampagoSync: falta o Particle System dos raios.");
        if (jogador == null) Debug.LogWarning("RelampagoSync: falta o Jogador (sem ele o raio não aparece).");
    }

    void Update()
    {
        // caixinha de teste do Inspector
        if (testarAgora)
        {
            testarAgora = false;
            Disparar(0);
        }

        if (videoPlayer == null || !videoPlayer.isPlaying) return;

        double tempoAtual = videoPlayer.time;

        // se o tempo voltou, o vídeo reiniciou (loop): recomeça a contagem
        if (tempoAtual < tempoAnterior) tempoAnterior = -0.01;

        for (int i = 0; i < temposDosRelampagos.Length; i++)
        {
            float t = temposDosRelampagos[i];
            if (t > tempoAnterior && t <= tempoAtual)
            {
                Disparar(i);
            }
        }

        tempoAnterior = tempoAtual;
    }

    // Teste rápido: no Play Mode, clique nos três pontinhos do componente e escolha "Testar relâmpago agora"
    [ContextMenu("Testar relâmpago agora")]
    void TestarAgora()
    {
        Disparar(0);
    }

    void Disparar(int indice)
    {
        Debug.Log("RelampagoSync: relâmpago disparado (#" + indice + ")");

        // posição do raio ao redor do jogador
        float graus = (angulosEmGraus != null && indice < angulosEmGraus.Length)
            ? angulosEmGraus[indice]
            : Random.Range(0f, 360f);
        float rad = graus * Mathf.Deg2Rad;

        if (raios != null && jogador != null)
        {
            Vector3 pos = jogador.position + new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)) * distanciaDoRaio;
            pos.y += alturaDoRaio;
            raios.transform.position = pos;
            if (tocarEfeitoCompleto)
            {
                raios.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                raios.Play(true);
                StartCoroutine(PararEfeitoDepois());
            }
            else
            {
                raios.Emit(1);
            }
        }

        if (piscando != null) StopCoroutine(piscando);
        piscando = StartCoroutine(PiscarLuz());
        StartCoroutine(TocarTrovao());
    }

    IEnumerator PararEfeitoDepois()
    {
        yield return new WaitForSeconds(duracaoDoEfeito);
        if (raios != null) raios.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    IEnumerator PiscarLuz()
    {
        if (luzRelampago == null) yield break;

        // padrão de "tremida" do relâmpago: forte, fraco, forte, apaga
        float[] padrao = { 1f, 0.2f, 0.8f, 0f };
        foreach (float p in padrao)
        {
            luzRelampago.intensity = intensidadeMaxima * p;
            yield return new WaitForSeconds(Random.Range(0.04f, 0.09f));
        }
        luzRelampago.intensity = 0f;
    }

    IEnumerator TocarTrovao()
    {
        if (audioTrovao == null || sonsDeTrovao == null || sonsDeTrovao.Length == 0) yield break;

        yield return new WaitForSeconds(Random.Range(atrasoDoTrovao.x, atrasoDoTrovao.y));
        audioTrovao.PlayOneShot(sonsDeTrovao[Random.Range(0, sonsDeTrovao.Length)]);
    }
}