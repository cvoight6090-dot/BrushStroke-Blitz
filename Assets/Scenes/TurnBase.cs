using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class TurnBase : MonoBehaviour
{

    public enum BattleState {PlayerTurn, Resolving, Won, Lost}

    [Header("Inscribed")]
    public Entity player;
    public List<Entity> enemies;

    //red,blue,yellow,back buttons
    public Button[] moveButtons;
    public Button backButton;

    //ui move anouncements
    public TMP_Text message;

    public float turnDelay = 1f;

    [Header("Dynamic")]
    public BattleState state;
    public ElementColor pickedColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (backButton != null) 
        {
            backButton.onClick.AddListener(BackPressed);
        }
        StartPlayerTurn();
    }

    void StartPlayerTurn() 
    {
        ButtonsInteractable(true);
        BaseColorMenu();
    }

    void BaseColorMenu() 
    {
        state = BattleState.PlayerTurn;
        messageText.text = "Pick a Base Color!";
        setBackButton(false);

        ElementColor[] colors = {ElementColor.Red, ElementColor.Yellow, ElementColor.Blue};
        for (int i = 0; i < moveButtons.Length; i++) 
        {
            bool used = i < colors.Length;
            moveButtons[i].gameObject.setActive();
            if (!used) continue;

            //possibly fix later for color changes
            ElementColor color = colors[i];
            SetupButton(moveButtons[i], color.ToString(), BaseToUnityColor(color),
                        () => OnBaseColorSelected(color));
        }

    }

    void OnBaseColorSelected() 
    {
        if (state != BattleState.PlayerTurn) return;
        ShowMoveMenu(color);
    }

    void ShowMoveMenu(ElementColor baseColor)
    {
        pickedColor = baseColor;
        messageText.text = "Pick a " + baseColor + "move!";
        setBackButton(true);

        List<MoveData> options = GetMovesFor(baseColor);
        for(int i = 0; i < moveButtons.Length; i++)
        {
            bool used = i < options.Count;
            moveButtons[i].gameObject.SetActive(used);
            if (!used) continue;

            MoveData move = options[i];
            SetupButton(moveButtons[i], move.moveName, move.buttonColor,
                        () => OnMoveSelected(move));
        }
    }

    void BackPressed() 
    {
        if (state != BattleState.PlayerTurn) return;
        BaseColorMenu();
    }
    List<MoveData> GetMovesFor(ElementColor baseColor) 
    {
        List<MoveData> options = new List<MoveData>();
        foreach (MoveData move in player.moves)
        {
            if (System.Array.IndexOf(move.usableFrom, baseColor) >= 0)
            {
                options.Add(move);
            }
        }
        return options;
    }
    void OnMoveSelected(MoveData move) 
    {
        if (state != BattleState.PlayerTurn) return;
        //coroutine??
        StartCoroutine(TurnRoutine(move));
    }

    IEnumerator TurnRoutine(MoveData playerMove) 
    {
        state = BattleState.Resolving;
        ButtonsInteractable(false);
        setBackButton(false);

        Entity target = GetFirstLivingEnemy();
        yield return UseMove(player, target, playerMove);

        if (GetFirstLivingEnemy() == null)
        {
            //change later
            EndBattle(BattleState.Won, "You Won!!");
            yield break;
        }

        foreach (Entity enemy in enemies)
        {
            if (enemy.health.IsDead || enemy.moves.Length == 0) continue;

            MoveData enemyMove = enemy.moves[Random.Range(0,enemy.moves.Length)];
            yield return UseMove(enemy, player, enemyMove);

            if (player.health.IsDead)
            {
                EndBattle(BattleState.Lost, "You Lost");
                yield break;
            }
        }
        StartPlayerTurn();
    }

    IEnumerator UseMove(Entity ant user, Entity target, MoveData move)
    {
        if (move.isHeal)
        {
            user.Health.Heal(move.power);
            messageText.text = user.displayName + " used " + move.moveName + "!";
        }
        else
        {
            target.health.takeDamage(move.power);
            messageText.text = user.displayName + " used " + move.moveName + "!";
        }
        //ask
        yield return new WaitForSeconds(turnDelay);

        if (!move.isHeal && target.health.isDead)
        {
            messageText.text = target.displayName + " fainted!";
            yield return new WaitForSeconds(turnDelay);
        }
    }

    Entity GetFirstLivingEnemy() 
    {
        foreach (Entity enemy in enemies)
        {
            if (enemy.health.isDead) return enemy;
        }
        return null;
    }

    void SetupButton(Button button, string label, Color tint, UnityAction onClick) 
    {
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null) text.text = label;

        button.image.color = tint;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);
    }

    Color BaseToUnityColor(ElementColor color) 
    {
        switch (color)
        {
            case ElementColor.Red: return Color.red;
            case ElementColor.Yellow: return Color.yellow;
            default: return Color.blue;
        }
    }

    void ButtonsInteractable(bool val) 
    {
        foreach (Button button in moveButtons)
        {
            button.interactable = val;
        }
        if (backButton != null) backButton.interactable = val;
    }

    void setBackButton(bool show)
    {
        if (backButton != null) backButton.gameObject.SetActive(show);
    }

    void EndBattle(BattleState result, string message)
    {
        state = result;
        messageText.text = message;
        ButtonsInteractable(false);
        setBackButton(false);
    }
}
