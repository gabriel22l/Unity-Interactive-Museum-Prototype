using UnityEngine;
using UnityEngine.VFX;

public class DissolveEffectHandler : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] private VisualEffect vfxGraph;
    private Material material;
    private int dissolveAmountID = Shader.PropertyToID("_DissolveAmount");
    private int colorID = Shader.PropertyToID("Color");
    private int edgeColorID = Shader.PropertyToID("_EdgeColor");
    private int vfxDurationID = Shader.PropertyToID("Duration");

    [SerializeField] private float effectDelay = 1f;
    [SerializeField] private float effectDuration = 4f;
    
    private Coroutine dissolveCoroutine;

    [SerializeField] [ColorUsage(true, true)] private Color dissolveColor = Color.red;
    private void Awake()
    {
        material = skinnedMeshRenderer.material;
        material.SetColor(edgeColorID, dissolveColor);
        
        vfxGraph.SetVector4(colorID, dissolveColor);

        float calculatedDuration = (effectDelay + effectDuration) * 0.6f; //hardcoded 60% for now
        vfxGraph.SetFloat(vfxDurationID, calculatedDuration);
    }
    public void DissolveShader()
    {
        if(dissolveCoroutine !=  null)
            StopCoroutine(dissolveCoroutine);
        dissolveCoroutine = StartCoroutine(DissolveEffect());
    }
    private System.Collections.IEnumerator DissolveEffect()
    {
        vfxGraph.Play();
        yield return new WaitForSeconds(effectDelay);
        float t = 0;
        while (t < effectDuration)
        {
            t+= Time.deltaTime;
            float amount = Mathf.Lerp(0f, 1f, t / effectDuration);
            material.SetFloat(dissolveAmountID, amount);
            yield return null;
        }
        material.SetFloat(dissolveAmountID, 1);
    }

    private void OnDisable()
    {
        if(material)
            Destroy(material);
        StopAllCoroutines();
    }
}
