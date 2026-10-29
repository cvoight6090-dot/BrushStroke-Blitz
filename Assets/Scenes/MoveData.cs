using UnityEngine;

public enum ElementColor {Red, Yellow, Blue, Orange, Green, Purple}
//creating moves as data and not code
[CreateAssetMenu(fileName = "NewMove", menuName = "Battle/Move")]

public class MoveData : ScriptableObject
{
    [Header("Inscribed")]
    public string moveName = "";
    public int damage = 2;
    [SerializeField] public AudioClip sound;

    public bool isHeal = false;

    //Which base colors can reach this move. 
    //ex: Red and Yellow can reach Orange but Blue cannot
    public ElementColor[] usableFrom;

    public Color buttonColor = Color.white;

    [Header("Color Type")]
    public ElementColor moveColor = ElementColor.Red;

    [Header("Area of Effect")]
    public bool isAOE = false;

    [Header("Shield")]
    public bool givesShield = false;

    [Header("Damage Over Time")]
    public int perTurnDOT = 0;
    public int turnsOfDOT = 0;
}
