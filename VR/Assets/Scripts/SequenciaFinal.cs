using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

// Coloque no objeto "Tempestade". O método Iniciar() é chamado pelo evento "Ao Ligar" do TVController.
// Sequência: raio cai no ponto marcado -> chuva para -> skybox troca -> objeto do final aparece.
public class SequenciaFinal : MonoBehaviour
{
    [Header("Raio final")]
    [SerializeField] private Transform pontoDoRaio;           // objeto vazio no meio do cenário, onde o raio cai
    [SerializeField] private ParticleSystem raio;             // pode ser o mesmo Particle System "Raios"
    [SerializeField] private RaioProcedural raioProcedural;   // (Opcional) raio desenhado por script, no lugar do Particle System
    [SerializeField] private float alturaDoCentroDoRaio = 20f; // metade da altura do raio, para ele tocar o chão
    [SerializeField] private Light luzRelampago;              // a mesma luz usada nos relâmpagos
    [SerializeField] private float intensidadeDoFlash = 6f;
    [Tooltip("Ligado: toca o efeito inteiro (efeitos prontos com vários Particle Systems). Desligado: emite só 1 partícula")]
    [SerializeField] private bool tocarEfeitoCompleto = true;
    [SerializeField] private float duracaoDoEfeito = 1f;
    [SerializeField] private AudioSource audioTrovao;
    [SerializeField] private AudioClip trovaoForte;
    [SerializeField] private float atrasoAntesDoRaio = 2f;

    [Header("Fim da tempestade")]
    [SerializeField] private RelampagoSync relampagosAleatorios; // é desligado para não cair mais raios
    [SerializeField] private ParticleSystem chuva;
    [SerializeField] private AudioSource somDaChuva;
    [SerializeField] private float duracaoFadeChuva = 3f;
    [SerializeField] private AudioSource somCeuCalmo;         // (Opcional) vento, pássaros, silêncio suave...

    [Header("Skybox (use UMA das opções)")]
    [SerializeField] private VideoPlayer videoSkybox;
    [SerializeField] private VideoClip clipeCeuCalmo;         // opção 1: segundo vídeo
    [SerializeField] private Renderer esferaSkybox;           // opção 2: troca o material da esfera
    [SerializeField] private Material materialCeuCalmo;
    [Tooltip("Opção 3: duas esferas. A da tempestade é desligada e a do céu calmo (que começa desligada) é ligada")]
    [SerializeField] private GameObject esferaTempestade;
    [SerializeField] private GameObject esferaCeuCalmo;

    [Header("Final")]
    [SerializeField] private GameObject objetoFinal;          // começa desligado e aparece depois do raio
    [SerializeField] private float atrasoParaObjetoAparecer = 1.5f;
    public UnityEvent aoTerminar;

    private bool iniciada;

    void Start()
    {
        if (objetoFinal != null) objetoFinal.SetActive(false);
        if (esferaCeuCalmo != null) esferaCeuCalmo.SetActive(false);
    }

    public void Iniciar()
    {
        if (iniciada) return;
        iniciada = true;
        StartCoroutine(Rotina());
    }

    IEnumerator Rotina()
    {
        // para os relâmpagos aleatórios do vídeo
        if (relampagosAleatorios != null) relampagosAleatorios.enabled = false;

        yield return new WaitForSeconds(atrasoAntesDoRaio);

        // 1. o raio cai
        if (raio != null && pontoDoRaio != null)
        {
            raio.transform.position = pontoDoRaio.position + Vector3.up * alturaDoCentroDoRaio;
            if (tocarEfeitoCompleto)
            {
                raio.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                raio.Play(true);
                StartCoroutine(PararRaioDepois());
            }
            else
            {
                raio.Emit(1);
            }
        }
        if (raioProcedural != null && pontoDoRaio != null) raioProcedural.Cair(pontoDoRaio.position);
        StartCoroutine(PiscarLuz());

        yield return new WaitForSeconds(0.15f);
        if (audioTrovao != null && trovaoForte != null) audioTrovao.PlayOneShot(trovaoForte);

        // 2. a tempestade acaba
        yield return new WaitForSeconds(1.2f);
        if (chuva != null) chuva.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        StartCoroutine(FadeOutChuva());
        TrocarCeu();
        if (somCeuCalmo != null) somCeuCalmo.Play();

        // 3. aparece o objeto do final
        yield return new WaitForSeconds(atrasoParaObjetoAparecer);
        if (objetoFinal != null) objetoFinal.SetActive(true);

        aoTerminar.Invoke();
    }

    IEnumerator PararRaioDepois()
    {
        yield return new WaitForSeconds(duracaoDoEfeito);
        if (raio != null) raio.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    IEnumerator PiscarLuz()
    {
        if (luzRelampago == null) yield break;

        float[] padrao = { 1f, 0.3f, 1f, 0.2f, 0.7f, 0.1f };
        foreach (float p in padrao)
        {
            luzRelampago.intensity = intensidadeDoFlash * p;
            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
        }
        luzRelampago.intensity = 0f;
    }

    IEnumerator FadeOutChuva()
    {
        if (somDaChuva == null) yield break;

        float inicial = somDaChuva.volume;
        float t = 0f;
        while (t < duracaoFadeChuva)
        {
            t += Time.deltaTime;
            somDaChuva.volume = Mathf.Lerp(inicial, 0f, t / duracaoFadeChuva);
            yield return null;
        }
        somDaChuva.Stop();
        somDaChuva.volume = inicial;
    }

    void TrocarCeu()
    {
        // opção 3: troca de esfera
        if (esferaTempestade != null) esferaTempestade.SetActive(false);
        if (esferaCeuCalmo != null) esferaCeuCalmo.SetActive(true);

        if (videoSkybox == null) return;

        videoSkybox.Stop();

        if (clipeCeuCalmo != null)
        {
            videoSkybox.clip = clipeCeuCalmo; // o VideoPlayer precisa estar com Source = Video Clip
            videoSkybox.isLooping = true;
            videoSkybox.Play();
        }
        else if (esferaSkybox != null && materialCeuCalmo != null)
        {
            esferaSkybox.material = materialCeuCalmo;
        }
    }
}
