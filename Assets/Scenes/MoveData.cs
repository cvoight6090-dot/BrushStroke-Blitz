using UnityEngine;

public enum ElementColor {Red, Yellow, Blue}
//creating moves as data and not code
[CreateAssetMenu(fileName = "NewMove", menuName = "Battle/Move")]

public class MoveData : ScriptableObject
{
    [Header("Inscribed")]
    public string moveName = "";
    public int damage = 2;

    public bool isHeal = false;

    //Which base colors can reach this move. 
    //ex: Red and Yellow can reach Orange but Blue cannot
    public ElementColor[] usableFrom;

    public Color buttonColor = Color.white;
}
