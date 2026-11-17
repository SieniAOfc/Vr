using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Coloque em um objeto vazio (ex.: "Tempestade"), NÃO no próprio painel, porque o painel é desligado no Start.
// O método Terminar() é chamado pelo evento "Ao Clicar" do objeto do final.
public class FimDeJogo : MonoBehaviour
{
    [SerializeField] private CanvasGroup painelFim;   // Canvas em World Space, filho da câmera, com fundo preto e o texto "FIM"
    [SerializeField] private float duracaoFade = 3f;
    [SerializeField] private AudioSource[] audiosParaSilenciar; // sons que somem junto com o fade
    [Tooltip("(Opcional) nome de uma cena para carregar depois do fim. Ela precisa estar em Build Profiles > Scene List")]
    [SerializeField] private string cenaParaCarregar = "";
    [SerializeField] private float tempoAteCarregar = 8f;

    [Header("Teste")]
    [SerializeField] private bool testarAgora; // no Play Mode, marque para testar o fim sem precisar jogar até lá

    private bool terminou;

    void Start()
    {
        if (painelFim != null)
        {
            painelFim.alpha = 0f;
            painelFim.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (testarAgora)
        {
            testarAgora = false;
            Terminar();
        }
    }

    public void Terminar()
    {
        if (terminou) return;
        terminou = true;
        StartCoroutine(Rotina());
    }

    IEnumerator Rotina()
    {
        // guarda os volumes originais para diminuir junto com o fade
        float[] volumes = new float[audiosParaSilenciar.Length];
        for (int i = 0; i < audiosParaSilenciar.Length; i++)
        {
            volumes[i] = audiosParaSilenciar[i] != null ? audiosParaSilenciar[i].volume : 0f;
        }

        if (painelFim != null) painelFim.gameObject.SetActive(true);

        float t = 0f;
        while (t < duracaoFade)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duracaoFade);

            if (painelFim != null) painelFim.alpha = k;
            for (int i = 0; i < audiosParaSilenciar.Length; i++)
            {
                if (audiosParaSilenciar[i] != null) audiosParaSilenciar[i].volume = volumes[i] * (1f - k);
            }
            yield return null;
        }

        if (painelFim != null) painelFim.alpha = 1f;

        if (!string.IsNullOrEmpty(cenaParaCarregar))
        {
            yield return new WaitForSeconds(tempoAteCarregar);
            SceneManager.LoadScene(cenaParaCarregar);
        }
    }
}
