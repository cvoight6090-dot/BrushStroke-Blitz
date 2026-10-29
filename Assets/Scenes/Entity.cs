using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Entity : MonoBehaviour
{

    private class DOTeffect
    {
        public string source;
        public int damage;
        public int turnsLeft;
    }

    [Header("Inscribed")]
    public string displayName = "Enemy";

    public ElementColor elementType = ElementColor.Red;
    public MoveData[] moves;

    public Health health{ get; private set;} 

    private List<DOTeffect> dot = new List<DOTeffect>();

    public bool haveDOT 
    {
        get {return dot.Count > 0; }
    }

    void Awake()
    {
        health = GetComponent<Health>();
    }

    public void AddDOT(string source, int dmg, int turns)
    {
        DOTeffect effect = new DOTeffect();
        effect.source = source;
        effect.damage = dmg;
        effect.turnsLeft = turns;
        dot.Add(effect);
    }

    public int TickDOT()
    {
        int total = 0;
        for (int i = dot.Count - 1; i >= 0; i--)
        {
            total += dot[i].damage;
            dot[i].turnsLeft--;
            
            if (dot[i].turnsLeft <= 0)
            {
                dot.RemoveAt(i);
            }
        }
        return total;
    }
}
