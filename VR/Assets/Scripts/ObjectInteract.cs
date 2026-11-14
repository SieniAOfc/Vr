using UnityEngine;
using UnityEngine.Events;

public class ObjectInteract : MonoBehaviour
{
     public Item item; //é o script Item com os dados deste objeto (se é pegavel e etc)

    public bool isMoving; //trava de segurança para saber se o objeto está sendo movido

    public UnityEvent CollectItem; //evento da Unity disparado ao coletar (ex: coletar copo)

    [Header("Uso de item (ex.: a TV que precisa da fita)")]
    public bool exigeItem;          //se verdadeiro, este objeto reage ao uso de um item do inventário (e não é coletado)
    public Item itemNecessario;     //item que o jogador precisa ter no inventário (ex.: Fita)
    public bool consumirItem = true; //se verdadeiro, o item sai do inventário depois de usado
    public bool usarUmaVez = true;  //se verdadeiro, só funciona uma vez
    public UnityEvent OnUsarItem;   //disparado quando o jogador interage TENDO o item (ex.: ligar a TV)
    public UnityEvent OnSemItem;    //disparado quando o jogador interage SEM o item (ex.: mensagem "falta algo")

    [HideInInspector] public bool jaUsado;
}
