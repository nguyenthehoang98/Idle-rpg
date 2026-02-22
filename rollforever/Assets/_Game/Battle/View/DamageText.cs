using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] private float duration = 1;
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshPro text;
    [SerializeField] private CanvasGroup canvasGroup;

    private Vector3 moveDir;
    private float timer;

    public void Init(string content,
        Color color, float scale)
    {
        text.text = content;
        text.color = color;

        transform.localScale = Vector3.one * scale;
        canvasGroup.alpha = 1f;

        moveDir = new Vector3(Random.Range(-0.3f, 0.3f), 1f, 0f);
        timer = 0f;
        gameObject.SetActive(true);
    }

    void Update()
    {
        timer += Time.deltaTime;

        transform.position += moveDir * Time.deltaTime;
        canvasGroup.alpha = 1f - timer / duration;

        if (timer >= duration)
        {
            DamageTextManager.Instance.Release(this);
        }
    }
}