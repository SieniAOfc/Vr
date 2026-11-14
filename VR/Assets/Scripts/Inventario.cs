using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventario : MonoBehaviour
{
   public List<Item> itens; //lista com todos os itens que o jogador possui

    // Evento disparado ao coletar um item novo (recebe o nome do item).
    // Útil para ligar uma UI depois, sem o script depender dela.
    public UnityEvent<string> OnItemAdded;

    public void AddItem(Item item)
    {
        //se o item já estiver na mochila, ignora para não repetir
        if (itens.Contains(item))
        {
            return;
        }

        itens.Add(item); //adiciona o item a lista lógica do inventário
        Debug.Log("Item coletado: " + item.InventarioNome);
        OnItemAdded.Invoke(item.InventarioNome);
    }
}
