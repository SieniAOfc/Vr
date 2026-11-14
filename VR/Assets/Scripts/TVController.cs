using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Coloque na TV. O método Ligar() é chamado pelo evento OnUsarItem do ObjectInteract da TV.
public class TVController : MonoBehaviour
{
    [Header("Tela")]
    [SerializeField] private Renderer telaRenderer;          // renderer da parte que é a tela
    [SerializeField] private int indiceMaterial = 0;         // qual material da tela será trocado
    [SerializeField] private Material materialTelaLigada;    // material "aceso" (emissivo, vídeo, chiado...)
    [SerializeField] private GameObject luzDaTela;           // (Opcional) Point Light que ilumina em volta da TV

    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip somInserirFita;       // som da fita entrando
    [SerializeField] private AudioClip somTvLigando;         // som da TV ligando

    [Header("Tempo")]
    [SerializeField] private float atrasoParaLigar = 1.5f;   // tempo entre inserir a fita e a TV ligar

    [Header("Eventos")]
    public UnityEvent aoLigar; // ex.: iniciar o vídeo da skybox, a chuva e o som da tempestade

    private bool ligada;

    public void Ligar()
    {
        if (ligada) return;
        ligada = true;
        StartCoroutine(SequenciaLigar());
    }

    IEnumerator SequenciaLigar()
    {
        if (audioSource != null && somInserirFita != null) audioSource.PlayOneShot(somInserirFita);

        yield return new WaitForSeconds(atrasoParaLigar);

        if (telaRenderer != null && materialTelaLigada != null)
        {
            Material[] mats = telaRenderer.materials;
            if (indiceMaterial >= 0 && indiceMaterial < mats.Length)
            {
                mats[indiceMaterial] = materialTelaLigada;
                telaRenderer.materials = mats;
            }
        }

        if (luzDaTela != null) luzDaTela.SetActive(true);
        if (audioSource != null && somTvLigando != null) audioSource.PlayOneShot(somTvLigando);

        aoLigar.Invoke();
    }
}
