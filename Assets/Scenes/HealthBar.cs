using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{

    [Header("Inscribed")]
    [Tooltip("Healthbar Owner:")]
    public Health health;

    public Image fillImage;

    public TMP_Text hpText;

    //tbd might remove
    [Tooltip("Bar Sliding Speed")]
    public float fillSpeed = 2f;

    private float targetFill = 1f;

    void OnEnable() 
    {
        if (health != null) health.OnHealthChanged += HandleHealthChanged;
    }
    void OnDisable() 
    {
        if (health != null) health.OnHealthChanged -= HandleHealthChanged;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (health == null)
        {
            Debug.LogWarning("Healthbar has not Health assigned");
            return;
        }

        targetFill = (float)health.currHP / health.maxHP;
        fillImage.fillAmount = targetFill;
        UpdateText(health.currHP, health.maxHP);
    }

    void HandleHealthChanged(int current, int max) 
    {
        targetFill = (float)current / max;
        UpdateText(current, max);
    }

    void UpdateText(int current, int max)
    {
        if (hpText != null)
        {
            hpText.text = current + "/" + max;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //slides for cleaner movement when damaged
        fillImage.fillAmount = Mathf.MoveTowards(
            fillImage.fillAmount, targetFill, fillSpeed * Time.deltaTime
        );
        //colors for full and empty. Green == full, red == empty
        fillImage.color = Color.Lerp(Color.red, Color.green, fillImage.fillAmount);
    }
}
