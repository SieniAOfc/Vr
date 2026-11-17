using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Coloque no MESMO objeto que tem o Collider: o CardboardReticlePointer manda as mensagens
// OnPointerEnter / OnPointerExit / OnPointerClick para o objeto atingido pelo raio.
// O objeto também precisa estar na layer marcada em "Reticle Interaction Layer Mask" do reticle.
//
// Faz o papel do antigo PlayerIntera: borda ao olhar, coletar item, usar item (TV) e clique genérico.
public class InteracaoCardboard : MonoBehaviour
{
    [Tooltip("Distância máxima (no chão) entre o jogador e o objeto para conseguir interagir")]
    [SerializeField] private float distanciaMaxima = 8f;
    [SerializeField] private float tempoEntreCliques = 0.4f;

    [Tooltip("Disparado ao clicar em objetos sem item (ex.: o objeto do final do jogo)")]
    public UnityEvent aoClicar;

    private ObjectInteract interact;      // opcional
    private ObjectHighlight highlight;    // opcional
    private Inventario inventario;
    private Transform jogador;
    private bool olhando;
    private float ultimoClique = -10f;
    private bool coletado;

    void Start()
    {
        interact = GetComponent<ObjectInteract>();
        highlight = GetComponent<ObjectHighlight>();
        inventario = FindFirstObjectByType<Inventario>();
        if (inventario != null) jogador = inventario.transform;
    }

    void Update()
    {
        // a borda só aparece se estiver olhando, perto o bastante e se o objeto ainda puder ser usado
        if (highlight != null)
        {
            highlight.SetHighlight(olhando && EstaPerto() && !Esgotado());
        }

        // Funciona no PC (Tecla E) e no Celular/PC com Controle Xbox (Botão A / Button South)
        bool apertouTeclado = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool apertouControle = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

        if (olhando && (apertouTeclado || apertouControle))
        {
            OnPointerClick();
        }
    }

    void OnDisable()
    {
        olhando = false;
        if (highlight != null) highlight.SetHighlight(false);
    }

    // ---- Mensagens enviadas pelo CardboardReticlePointer ----

    public void OnPointerEnter()
    {
        olhando = true;
    }

    public void OnPointerExit()
    {
        olhando = false;
    }

    public void OnPointerClick()
    {
        if (coletado || Time.time - ultimoClique < tempoEntreCliques) return;
        if (!EstaPerto() || Esgotado()) return;
        ultimoClique = Time.time;

        // Objeto que exige um item (ex.: TV com a fita)
        if (interact != null && interact.exigeItem)
        {
            UsarItem();
            return;
        }

        // Item de coleta direta (ex.: a fita)
        if (interact != null && interact.item != null && interact.item.coletarDireto)
        {
            Coletar();
            return;
        }

        // Qualquer outro objeto: só dispara o evento do Inspector
        aoClicar.Invoke();
    }

    // ---- Lógica ----

    void UsarItem()
    {
        if (inventario != null && inventario.TemItem(interact.itemNecessario))
        {
            interact.jaUsado = true;
            if (interact.consumirItem) inventario.RemoverItem(interact.itemNecessario);
            interact.OnUsarItem.Invoke(); // ex.: TVController.Ligar()
        }
        else
        {
            interact.OnSemItem.Invoke(); // ex.: tocar um som de "falta algo"
        }
    }

    void Coletar()
    {
        if (highlight != null) highlight.SetHighlight(false); // tira a borda antes de sumir com o objeto
        olhando = false;

        if (interact.item.inventoryItem && inventario != null)
        {
            inventario.AddItem(interact.item);
        }

        interact.CollectItem.Invoke();

        // Esconde o objeto SEM desativá-lo. O CardboardReticlePointer ainda vai mandar OnPointerExit
        // para ele, e um objeto desativado não tem quem receba a mensagem (erro "has no receiver").
        coletado = true;
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;

        // Depois de um instante (o reticle já mandou o OnPointerExit), desativa o objeto de vez
        StartCoroutine(DesativarDepois(0.3f));
    }

    IEnumerator DesativarDepois(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        gameObject.SetActive(false);
    }

    bool EstaPerto()
    {
        if (jogador == null) return true;

        Vector3 a = jogador.position;
        Vector3 b = transform.position;
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b) <= distanciaMaxima;
    }

    bool Esgotado()
    {
        return interact != null && interact.exigeItem && interact.usarUmaVez && interact.jaUsado;
    }
}