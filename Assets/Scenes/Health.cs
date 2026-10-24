using System;
using UnityEngine;

public class Health : MonoBehaviour
{

    [SerializeField] private AudioClip damageSound;

    [Header("Inscribed")]
    public int maxHP = 10;

    [Header("Dynamic")]
    public int currHP;

    public event Action<int, int> OnHealthChanged;

    public event Action OnDied;
    
    public bool IsDead
    {
        get {return currHP <= 0;}
    }

    void Awake()
    {
        currHP = maxHP;
    }

    public void takeDamage(int amount)
    {
        if (IsDead) return;

        amount = Mathf.Max(0,amount);
        currHP = Mathf.Max(0, currHP - amount);
        OnHealthChanged?.Invoke(currHP,maxHP);
        SoundFXManager.instance.PlaySoundFXCLip(damageSound, transform, 1f);

        if (currHP == 0)
        {
            OnDied?.Invoke();
        }
    }
    public void Heal(int amount)
    {
        if (IsDead) return;

        amount = Mathf.Max(0,amount);
        currHP = Mathf.Max(0, currHP + amount);
        //idk if neccessary
        if (currHP > maxHP) currHP = maxHP;
        OnHealthChanged?.Invoke(currHP, maxHP);
    }
}
