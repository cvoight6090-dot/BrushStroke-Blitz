using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class ColorThemedImage : MonoBehaviour
{

    [Header("Inscribed")]
    public TurnBase turnBase;

    [Tooltip("0 = never changes, 1 = changes to full picked color")]
    [Range(0f,1f)]
    public float strength = 1f;

    private Image image;
    private Color nuetralColor;

    void Awake()
    {
        image = GetComponent<Image>(); 
        //no change color
        nuetralColor = image.color;
    }

    void OnEnable()
    {
        if (turnBase == null) return;
        turnBase.OnBaseColorPicked += HandlePicked;
        turnBase.OnBaseColorCleared += HandleCleared;
    }
    void OnDisable()
    {
        if (turnBase == null) return;
        turnBase.OnBaseColorPicked -= HandlePicked;
        turnBase.OnBaseColorCleared -= HandleCleared;
    }  
    void HandlePicked(ElementColor element, Color pickedColor)
    {
        Color blended = Color.Lerp(nuetralColor, pickedColor, strength);
        blended.a = nuetralColor.a; //keep original transparency
        image.color = blended;
    }

    void HandleCleared()
    {
        image.color = nuetralColor;
    }
}
