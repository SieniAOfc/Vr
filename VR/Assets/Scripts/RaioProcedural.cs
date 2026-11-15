using System.Collections;
using UnityEngine;

// Desenha um raio em zigue-zague do céu até um ponto no chão, piscando por um instante.
// Não precisa de textura nem de efeito pronto. Coloque em um objeto vazio (o LineRenderer é adicionado sozinho).
// É chamado pelo RelampagoSync e pelo SequenciaFinal, mas também dá para testar pela caixinha "Testar Agora".
[RequireComponent(typeof(LineRenderer))]
public class RaioProcedural : MonoBehaviour
{
    [Header("Forma")]
    [SerializeField] private float altura = 80f;          // comprimento do raio
    [SerializeField] private int segmentos = 24;          // mais segmentos = mais detalhe no zigue-zague
    [SerializeField] private float zigueZague = 4f;       // o quanto o raio "treme" para os lados
    [SerializeField] private float desvioNoTopo = 10f;    // o topo do raio sai um pouco fora do prumo
    [Tooltip("Desligado: a posição é onde o raio TOCA O CHÃO (ele sobe a partir dali). Ligado: a posição é o TOPO do raio (ele desce a partir dali). Para o RelampagoSync e o SequenciaFinal, deixe desligado")]
    [SerializeField] private bool posicaoEhOTopo = false;

    [Header("Aparência")]
    [SerializeField] private float larguraMaxima = 1.5f;
    [SerializeField] private Color cor = new Color(0.85f, 0.92f, 1f, 1f);
    [Tooltip("Ideal: material com shader Universal Render Pipeline/Unlit. Se vazio, usa um material simples.")]
    [SerializeField] private Material material;
    [SerializeField] private int quantidadePiscadas = 4;

    [Header("Impacto (opcional)")]
    [Tooltip("Particle System de faíscas que toca no ponto onde o raio bate no chão")]
    [SerializeField] private ParticleSystem faiscasNoImpacto;

    [Header("Teste")]
    [SerializeField] private bool testarAgora; // no Play Mode, marque para o raio cair usando a posição deste objeto

    private LineRenderer lr;
    private Coroutine rotina;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.numCapVertices = 2;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.startColor = cor;
        lr.endColor = cor;

        if (material != null)
        {
            lr.sharedMaterial = material;
        }
        else
        {
            Shader sh = Shader.Find("Sprites/Default");
            if (sh != null) lr.sharedMaterial = new Material(sh);
            else Debug.LogWarning("RaioProcedural: atribua um material (shader Unlit) no campo Material.");
        }

        lr.enabled = false;
    }

    void Update()
    {
        if (testarAgora)
        {
            testarAgora = false;
            Cair(transform.position);
        }
    }

    // Dispara o raio. "ponto" é o ponto do chão (ou o topo, se "posicaoEhOTopo" estiver ligado)
    public void Cair(Vector3 ponto)
    {
        Vector3 topo = posicaoEhOTopo ? ponto : ponto + Vector3.up * altura;
        Vector3 baixo = posicaoEhOTopo ? ponto + Vector3.down * altura : ponto;

        if (rotina != null) StopCoroutine(rotina);
        rotina = StartCoroutine(Piscar(topo, baixo));

        if (faiscasNoImpacto != null)
        {
            faiscasNoImpacto.transform.position = baixo;
            faiscasNoImpacto.Play(true);
        }
    }

    IEnumerator Piscar(Vector3 topo, Vector3 baixo)
    {
        lr.enabled = true;

        for (int i = 0; i < quantidadePiscadas; i++)
        {
            GerarRaio(topo, baixo);

            float largura = larguraMaxima * Random.Range(0.6f, 1f);
            lr.startWidth = largura;      // mais grosso no topo
            lr.endWidth = largura * 0.4f; // mais fino onde bate no chão

            yield return new WaitForSeconds(Random.Range(0.03f, 0.07f));

            // um instante apagado entre as piscadas
            if (i < quantidadePiscadas - 1)
            {
                lr.enabled = false;
                yield return new WaitForSeconds(Random.Range(0.02f, 0.05f));
                lr.enabled = true;
            }
        }

        lr.enabled = false;
    }

    void GerarRaio(Vector3 topo, Vector3 baixo)
    {
        int n = Mathf.Max(2, segmentos);

        Vector3 inicio = topo + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) * desvioNoTopo;

        lr.positionCount = n + 1;
        for (int i = 0; i <= n; i++)
        {
            float t = i / (float)n;
            Vector3 p = Vector3.Lerp(inicio, baixo, t);

            // zigue-zague: desvio lateral aleatório, que some nas pontas (o raio começa e termina nos pontos certos)
            if (i != 0 && i != n)
            {
                float fator = Mathf.Sin(t * Mathf.PI);
                p += new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) * zigueZague * fator;
            }

            lr.SetPosition(i, p);
        }
    }
}