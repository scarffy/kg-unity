using UnityEngine;
using UnityEngine.VFX;

[ExecuteInEditMode]
public class SetProps : MonoBehaviour
{
    public Transform emptyBall;
    public Material material;
    public float noisePower = 0.5f;
    public float noiseSize = 1f;
    
    private VisualEffect effect;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        effect  = GetComponent<VisualEffect>();
    }

    // Update is called once per frame
    void Update()
    {
        if (emptyBall && material)
        {
            effect.SetVector3("Ball Pos", emptyBall.position);
            effect.SetFloat("Ball Size", emptyBall.localScale.x);
            effect.SetFloat("Noise Size", noiseSize);
            effect.SetFloat("Noise Power", noisePower);
            
            material.SetVector("Ball_Pos", emptyBall.position);
            material.SetFloat("Ball_Size", emptyBall.localScale.x);
            material.SetFloat("Noise_Size", noiseSize);
            material.SetFloat("Noise_Power", noisePower);
        }
    }
}
