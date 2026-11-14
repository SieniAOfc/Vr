using UnityEngine;

public class bombA : MonoBehaviour
{

    public RenderTexture ceuChuva;

    void Start()
    {
        // Limpa o frame preto inicial da Render Texture
        ceuChuva.Release();
    }
}

