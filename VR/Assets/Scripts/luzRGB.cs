using UnityEngine;

[RequireComponent(typeof(Light))]
public class luzRGB : MonoBehaviour
{
    [Tooltip("Velocidade com que as cores mudam")]
    public float velocidade = 0.2f;

    [Range(0f, 1f)]
    public float saturacao = 1f;

    [Range(0f, 1f)]
    public float brilho = 1f;

    private Light luz;
    private float matiz;

    void Start()
    {
        luz = GetComponent<Light>();
    }

    void Update()
    {
        // Incrementa o valor do matiz (Hue) ao longo do tempo e reinicia ao chegar em 1
        matiz += velocidade * Time.deltaTime;
        if (matiz > 1f)
        {
            matiz -= 1f;
        }

        // Converte HSV para RGB e aplica na luz
        luz.color = Color.HSVToRGB(matiz, saturacao, brilho);
    }
}
