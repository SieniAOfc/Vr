using System.Collections.Generic;
using UnityEngine;

// Coloque este script no MESMO objeto que tem o ObjectInteract.
// Ele adiciona o material de contorno aos renderers do objeto quando o jogador olha para ele.
public class ObjectHighlight : MonoBehaviour
{
    [SerializeField] private Material outlineMaterial; // material criado com o shader Custom/OutlineURP

    private Renderer[] renderers;
    private bool highlighted;

    void Awake()
    {
        // pega o renderer do objeto e de todos os filhos (modelos importados costumam ter vários)
        renderers = GetComponentsInChildren<Renderer>();
    }

    public void SetHighlight(bool on)
    {
        if (on == highlighted || outlineMaterial == null) return;
        highlighted = on;

        foreach (Renderer r in renderers)
        {
            List<Material> mats = new List<Material>(r.sharedMaterials);

            if (on) mats.Add(outlineMaterial);
            else mats.Remove(outlineMaterial);

            r.sharedMaterials = mats.ToArray();
        }
    }
}
