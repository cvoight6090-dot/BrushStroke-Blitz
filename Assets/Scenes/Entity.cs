using UnityEngine;

[RequireComponent(typeof(Health))]
public class Entity : MonoBehaviour
{
    [Header("Inscribed")]
    public string displayName = "Enemy";
    public MoveData[] moves;

    public Health health{ get; private set;} 

    void Awake()
    {
        health = GetComponent<Health>();
    }

}
