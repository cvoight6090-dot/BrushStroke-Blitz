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
        message.text = "Pick a Base Color!";
        setBackButton(false);

        ElementColor[] colors = {ElementColor.Red, ElementColor.Yellow, ElementColor.Blue};
        for (int i = 0; i < moveButtons.Length; i++) 
        {
            bool used = i < colors.Length;
            moveButtons[i].gameObject.SetActive(used);
            if (!used) continue;

            //possibly fix later for color changes
            ElementColor color = colors[i];
            SetupButton(moveButtons[i], color.ToString(), BaseToUnityColor(color),
                        () => OnBaseColorSelected(color));
        }

    }

    void OnBaseColorSelected(ElementColor color) 
    {
        if (state != BattleState.PlayerTurn) return;
        ShowMoveMenu(color);
    }

    void ShowMoveMenu(ElementColor baseColor)
    {
        pickedColor = baseColor;
        message.text = "Pick a " + baseColor + "move!";
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

        yield return TickDamageOverTime();

        if (GetFirstLivingEnemy() == null)
        {
            //change later
            EndBattle(BattleState.Won, "You Won!!");
            yield break;
        }
        if (player.health.IsDead)
        {
            EndBattle(BattleState.Lost, "You Lost");
            yield break;
        }

        StartPlayerTurn();
    }

    IEnumerator UseMove(Entity user, Entity target, MoveData move)
    {
        List<Entity> targets = new List<Entity>();
        if (move.sound != null)
        {
            SoundFXManager.instance.PlaySoundFXCLip(move.sound, transform, 2f);
        }
        yield return new WaitForSeconds(turnDelay);
        if (move.isHeal)
        {
            user.health.Heal(move.damage);
            message.text = user.displayName + " used " + move.moveName + " and healed!";
            yield return new WaitForSeconds(turnDelay);
            
            if (user == player && move.isAOE)
            {
               foreach(Entity enemy in enemies)
                {
                if (!enemy.health.IsDead) targets.Add(enemy);
                }
                foreach (Entity t in targets)
                {
                    if (move.turnsOfDOT > 0 && move.perTurnDOT > 0)
                    {
                    t.AddDOT(move.moveName, move.perTurnDOT, move.turnsOfDOT);
                    yield return new WaitForSeconds(turnDelay);
                    }
                }
            }
            
            yield break;
        }

        
        if (user == player && move.isAOE)
        {
            foreach(Entity enemy in enemies)
            {
                if (!enemy.health.IsDead) targets.Add(enemy);
            }
        }
        else
        {
            targets.Add(target);
        }


        foreach (Entity t in targets)
        {
            if (move.damage > 0)
            {
                t.health.takeDamage(move.damage);
                message.text = user.displayName + " used " + move.moveName + "!";
            }
            if (move.turnsOfDOT > 0 && move.perTurnDOT > 0)
            {
                t.AddDOT(move.moveName, move.perTurnDOT, move.turnsOfDOT);
            }
        }
        //ask
        yield return new WaitForSeconds(turnDelay);

        foreach(Entity t in targets)
        {
            if (t.health.IsDead)
            {
                message.text = t.displayName + " fainted!";
                yield return new WaitForSeconds(turnDelay);
                if (enemies.Contains(t))
                {
                    RemoveEnemy(t);
                }
            }
        }
    }

    IEnumerator TickDamageOverTime() 
    {
        List<Entity> everyone = new List<Entity>();
        everyone.Add(player);
        everyone.AddRange(enemies);

        foreach(Entity e in everyone)
        {
            if (e.health.IsDead || !e.haveDOT) continue;

            int damage = e.TickDOT();
            e.health.takeDamage(damage);
            message.text = e.displayName + " took " + damage + " damage over time!";
            
            yield return new WaitForSeconds(turnDelay);

            if (e.health.IsDead)
            {
                message.text = e.displayName + " fainted!";
                yield return new WaitForSeconds(turnDelay);
            }
            
        }
    }

    Entity GetFirstLivingEnemy() 
    {
        foreach (Entity enemy in enemies)
        {
            if (!enemy.health.IsDead) return enemy;
        }
        return null;
    }

    void RemoveEnemy(Entity enemy) 
    {
        enemies.Remove(enemy);
        Destroy(enemy.gameObject);
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

    void EndBattle(BattleState result, string msg)
    {
        state = result;
        message.text = msg;
        ButtonsInteractable(false);
        setBackButton(false);
    }
}
