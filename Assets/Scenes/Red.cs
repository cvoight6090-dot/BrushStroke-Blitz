using UnityEngine;
using UnityEngine.UI;

public class Red : MonoBehaviour
{

    public Button redButton;
    public Health target;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (redButton != null) 
        {
            GameObject player = GameObject.Find("Player");
            target = player.GetComponent<Health>();
            redButton.onClick.AddListener(redButtonClicked);
        }
    }
    void redButtonClicked()
    {
        target.takeDamage(2);
        Debug.LogWarning("Button Pressed");
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
