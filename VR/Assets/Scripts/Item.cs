using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class Item : ScriptableObject
{
    public bool pegavel;
    public AudioClip audioClip;
    public string text;
    public Sprite image;

    [Header("Inventario")]
    public bool inventoryItem; //se for verdadeiro, o item vai para a lista da mochila ao ser interagido e sair da interação
    public bool coletarDireto; //se for verdadeiro, ao clicar o item vai direto para o inventário (sem modo de inspeção)
    public string collectMessage; //mensagem que aparece na tela quando você coleta o item
    public string InventarioNome; //nome que vai aparecer na lista do inventário

}