using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerIntera : MonoBehaviour
{

    public float rayDistance = 2f; // Distância máxima do raio de interação
    [SerializeField] private Camera myCam; // Câmera do jogador
    [SerializeField] private GameObject handCursor; // (Opcional) ícone de mão na tela quando mira em algo interagível
    [SerializeField] private GameObject backImage; // (Opcional) aviso de "aperte X para voltar" durante a inspeção

    [Header("Input (nomes das Actions no Input Actions asset)")]
    [SerializeField] private string interactActionName = "Attack"; // coletar / inspecionar
    [SerializeField] private string backActionName = "Crouch";     // soltar / coletar durante a inspeção
    [SerializeField] private string lookActionName = "Look";       // analógico direito: gira o objeto na inspeção
    private InputAction interactAction;
    private InputAction backAction;
    private InputAction lookAction;

    private ObjectInteract currentInteractable; // Objeto que você está segurando/olhando no momento
    private ObjectHighlight currentHighlight; // Objeto que está com a borda ligada no momento
    private Vector3 originPosition; // Posição original do objeto antes de ser pego
    private Quaternion originRotation; // Rotação original do objeto
    private bool isViewing; // Marca se o jogador está no modo "Inspecionando Objeto"
    public Transform objectViewer; // Ponto na frente da câmera onde o objeto fica flutuando
    public float rotateSpeed = 200; // Velocidade de rotação do objeto com o analógico
    public UnityEvent OnView; // Evento ao começar a olhar um objeto
    private bool canFinish; // Trava para impedir fechar a inspeção no mesmo clique que abriu
    public UnityEvent OnFinishView; // Evento ao terminar de olhar o objeto
    private Inventario inventario; // Referência ao inventário do jogador

    void Awake()
    {
        inventario = GetComponent<Inventario>(); // Pega o componente Inventario no mesmo objeto
    }

    void Start()
    {
        if (myCam == null) myCam = Camera.main; // Pega a câmera principal

        interactAction = FindAction(interactActionName);
        backAction = FindAction(backActionName);
        lookAction = FindAction(lookActionName);

        SetHandCursor(false);
        if (backImage != null) backImage.SetActive(false);
    }

    InputAction FindAction(string actionName)
    {
        InputAction action = InputSystem.actions.FindAction(actionName);
        if (action == null) Debug.LogError("PlayerIntera: não achei a Action '" + actionName + "' no Input Actions asset.");
        return action;
    }

    void Update()
    {
        CheckInteractables(); // Roda a checagem de raio a cada frame
    }

    void SetHandCursor(bool on)
    {
        if (handCursor != null) handCursor.SetActive(on);
    }

    // Liga a borda no objeto novo e desliga no anterior (passe null para desligar tudo)
    void UpdateHighlight(ObjectHighlight target)
    {
        if (currentHighlight == target) return;

        if (currentHighlight != null) currentHighlight.SetHighlight(false);
        currentHighlight = target;
        if (currentHighlight != null) currentHighlight.SetHighlight(true);
    }

    void CheckInteractables()
    {
        if (isViewing) // Se já estiver inspecionando um objeto bem de perto
        {
            UpdateHighlight(null);

            if (currentInteractable.item.pegavel)
            {
                RotateObject(); // Analógico direito gira o objeto
            }
            if (canFinish && backAction != null && backAction.WasPressedThisFrame())
            {
                FinishView(); // Solta/coleta o objeto
            }
            return; // Não dispara raios enquanto inspeciona
        }

        RaycastHit hit;
        Vector3 rayOrigin = myCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.5f)); // Centro da tela em mundo 3D

        if (Physics.Raycast(rayOrigin, myCam.transform.forward, out hit, rayDistance))
        {
            ObjectInteract interactable = hit.collider.GetComponentInParent<ObjectInteract>();
            if (interactable != null)
            {
                SetHandCursor(true);
                UpdateHighlight(interactable.GetComponent<ObjectHighlight>()); // liga a borda

                if (interactAction != null && interactAction.WasPressedThisFrame())
                {
                    if (interactable.isMoving)
                    {
                        return;
                    }

                    // Item de coleta direta: vai para o inventário no clique, sem inspeção
                    if (interactable.item.coletarDireto)
                    {
                        CollectDirectly(interactable);
                        return;
                    }

                    OnView.Invoke();
                    currentInteractable = interactable;

                    isViewing = true;

                    Invoke("CanFinish", 1f); // Libera o cancelamento após 1 segundo

                    if (currentInteractable.item.pegavel) // Salva onde o objeto estava antes de puxar para perto
                    {
                        originPosition = currentInteractable.transform.position;
                        originRotation = currentInteractable.transform.rotation;
                        StartCoroutine(MovingObject(currentInteractable, objectViewer.position));
                    }
                }
            }
            else
            {
                UpdateHighlight(null);
                SetHandCursor(false);
            }
        }
        else
        {
            UpdateHighlight(null);
            SetHandCursor(false);
        }
    }

    void CollectDirectly(ObjectInteract obj)
    {
        UpdateHighlight(null); // tira a borda antes de sumir com o objeto
        SetHandCursor(false);

        if (obj.item.inventoryItem)
        {
            inventario.AddItem(obj.item); // manda para o inventário
        }

        obj.CollectItem.Invoke(); // evento configurado no Inspector (som, mensagem, etc.)
        obj.gameObject.SetActive(false); // some com o objeto da cena
    }

    IEnumerator MovingObject(ObjectInteract obj, Vector3 position) // Move o objeto suavemente entre a mesa e o visor da câmera
    {
        obj.isMoving = true;
        float timer = 0;
        while (timer < 1)
        {
            obj.transform.position = Vector3.Lerp(obj.transform.position, position, Time.deltaTime * 5);
            timer += Time.deltaTime;
            yield return null;
        }
        obj.transform.position = position;
        obj.isMoving = false;
    }

    void RotateObject()
    {
        // Garante que só roda se houver um objeto selecionado e ele for pegável
        if (currentInteractable == null || !currentInteractable.item.pegavel || lookAction == null) return;

        Vector2 look = lookAction.ReadValue<Vector2>(); // analógico: valores de -1 a 1
        float x = Mathf.Clamp(look.x, -1f, 1f);
        float y = Mathf.Clamp(look.y, -1f, 1f);

        currentInteractable.transform.Rotate(myCam.transform.right, y * rotateSpeed * Time.deltaTime, Space.World);
        currentInteractable.transform.Rotate(myCam.transform.up, -x * rotateSpeed * Time.deltaTime, Space.World);
    }

    void CanFinish()
    {
        canFinish = true;
        if (backImage != null) backImage.SetActive(true);
    }

    void FinishView()
    {
        canFinish = false;
        isViewing = false;
        if (backImage != null) backImage.SetActive(false);

        if (currentInteractable.item.inventoryItem)
        {
            inventario.AddItem(currentInteractable.item);
            currentInteractable.CollectItem.Invoke();
        }

        if (currentInteractable.item.pegavel)
        {
            currentInteractable.transform.rotation = originRotation;
            StartCoroutine(MovingObject(currentInteractable, originPosition));
        }
        OnFinishView.Invoke();
    }

}