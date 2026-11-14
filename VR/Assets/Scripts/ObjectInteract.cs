using UnityEngine;
using UnityEngine.Events;

public class ObjectInteract : MonoBehaviour
{
    public Item item;              // dados do objeto (se é pegável etc.)
    public bool isMoving;          // trava para saber se o objeto está sendo movido
    public UnityEvent CollectItem; // evento disparado ao coletar
}
