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

    public Color shieldColor = new Color(0.25f, 0.55f, 1f);

    private float targetFill = 1f;
    private bool shielded = false;

    void OnEnable() 
    {
        if (health == null) return;

        health.OnHealthChanged += HandleHealthChanged;
        health.OnShieldChanged += HandleShieldChanged;
    }
    void OnDisable() 
    {
        if (health == null) return;
        
        health.OnHealthChanged -= HandleHealthChanged;
        health.OnShieldChanged -= HandleShieldChanged;
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
        shielded = health.HasShield;
        UpdateText(health.currHP, health.maxHP);
    }

    void HandleHealthChanged(int current, int max) 
    {
        targetFill = (float)current / max;
    }
    void HandleShieldChanged(bool hasShield)
    {
        shielded = hasShield;
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
        if (shielded) 
        {
            fillImage.color = shieldColor;
        }
        else 
        {
        //colors for full and empty. Green == full, red == empty
        fillImage.color = Color.Lerp(Color.red, Color.green, fillImage.fillAmount);
        }

        UpdateText(Mathf.RoundToInt(fillImage.fillAmount * health.maxHP), health.maxHP);
    }
}
