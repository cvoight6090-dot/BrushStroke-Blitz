using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Inscribed")]
    public int maxHP = 10;

    [Header("Dynamic")]
    public int currHP;

    public event Action<int, int> OnHealthChanged;

    public event Action OnDied;
    
    public bool isDead 
    {
        get {return currHP <= 0;}
    }

    void Awake()
    {
        currHP = maxHP;
    }

    public void takeDamage(int amount)
    {
        if (isDead) return;

        amount = Mathf.Max(0,amount);
        currHP = Mathf.Max(0, currHP - amount);
        OnHealthChanged?.Invoke(currHP,maxHP);

        if (currHP == 0)
        {
            OnDied?.Invoke();
        }
    }
    public void Heal(int amount)
    {
        if (isDead) return;

        amount = Mathf.Max(0,amount);
        currHP = Mathf.Max(0, currHP + amount);
        //idk if neccessary
        if (currHP > maxHP) currHP = maxHP;
        OnHealthChanged?.Invoke(currHP, maxHP);
    }
}
