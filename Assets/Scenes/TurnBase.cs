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

    public float turnDelay = 2f;

    //adjust this potentially
    [Header("Color Pallete")]
    public Color redColor = new Color(0.9f,0.2f,0.2f);
    public Color yellowColor = new Color(1f,0.85f,0.2f);
    public Color blueColor = new Color(0.2f,0.4f,0.9f);
    public Color orangeColor = new Color(1f,0.6f,0.2f);
    public Color greenColor = new Color(0.3f,0.75f,0.3f);
    public Color purpleColor = new Color(0.6f,0.3f,0.8f);

    [Header("Damage Multiplier Based on Color Wheel")]
    [Tooltip("Attack same as entity color")]
    public float sameMultiplier = 0.5f;

    [Tooltip("Attack neighbors entity color")]
    public float neighborMultiplier = 1f;

    [Tooltip("Attack neighbors to opposite entity color")]
    public float oppNeighborMultiplier = 2f;

    [Tooltip("Attack opposite of entity color")]
    public float oppositeMultiplier = 2f;

    
    [Header("Dynamic")]
    public BattleState state;
    public ElementColor pickedColor;

    //for other scripts to call,, monitor use
    public event System.Action<ElementColor, Color> OnBaseColorPicked;
    public event System.Action OnBaseColorCleared;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (backButton != null) 
        {
            backButton.onClick.AddListener(BackPressed);
        }
        StartPlayerTurn();
    }

    public Color GetColor(ElementColor color)
    {
        switch (color)
        {
            case ElementColor.Red: return redColor;
            case ElementColor.Yellow: return yellowColor;
            case ElementColor.Blue: return blueColor;
            case ElementColor.Orange: return orangeColor;
            case ElementColor.Green: return greenColor;
            default: return purpleColor;
        }
    }

    public float GetMatchupMult(ElementColor attackColor, ElementColor entityType)
    {
        switch (ColorWheel.Distance(attackColor, entityType))
        {
            case 0: return sameMultiplier;
            case 1: return neighborMultiplier;
            case 2: return oppNeighborMultiplier;
            default: return oppositeMultiplier;
        }
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
        OnBaseColorCleared?.Invoke();

        ElementColor[] colors = {ElementColor.Red, ElementColor.Yellow, ElementColor.Blue};
        for (int i = 0; i < moveButtons.Length; i++) 
        {
            bool used = i < colors.Length;
            moveButtons[i].gameObject.SetActive(used);
            if (!used) continue;

            //possibly fix later for color changes
            ElementColor color = colors[i];
            SetupButton(moveButtons[i], color.ToString(), GetColor(color),
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
        player.elementType = baseColor;
        OnBaseColorPicked?.Invoke(baseColor, GetColor(baseColor));
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
        if (move.sound != null)
        {
            SoundFXManager.instance.PlaySoundFXCLip(move.sound, transform, 2f);
        }
        yield return new WaitForSeconds(turnDelay);

        if (move.isHeal || move.givesShield)
        {
            List<Entity> helped = GetSupportTargets(user,move);
            foreach(Entity ally in helped)
            {
                if (move.isHeal) ally.health.Heal(move.damage);
                if (move.givesShield) ally.health.GiveShield();
            }

            string who = helped.Count > 1 ? "The whole team" : helped[0].displayName;
            string result;
            if (move.isHeal && move.givesShield) result = " was healed and got a shield!";
            else if (move.isHeal) result = " was healed!";
            else result = " got a shield!";

            message.text = user.displayName + " used " + move.moveName + "! " + who + result;
            yield return new WaitForSeconds(turnDelay);
            yield break;
        }
        List<Entity> targets = GetDamageTargets(user, target, move);

        if (targets.Count > 1)
        {
            message.text = user.displayName + " used " + move.moveName + 
            " on every enemy!";
        }
        else
        {
            message.text = user.displayName + " used " + move.moveName + 
            "!";
        }
        yield return new WaitForSeconds(turnDelay);

        bool dealsDOT = move.turnsOfDOT > 0 && move.perTurnDOT > 0;
        if (move.damage <= 0 && !dealsDOT) yield break;

        bool paused = false;

        foreach (Entity t in targets)
        {
            if (t.health.HasShield)
            {
                t.health.BreakShield();
                message.text = t.displayName + "'s shield blocked a hit!";
                yield return new WaitForSeconds(turnDelay);
                paused = true;
                continue;
            }
            float multiplier = GetMatchupMult(move.moveColor, t.elementType);

            if (move.damage > 0)
            {
                int damage = Mathf.Max(1, Mathf.RoundToInt(move.damage * multiplier));
                t.health.takeDamage(damage);
            }
            if (dealsDOT)
            {
                t.AddDOT(move.moveName, move.perTurnDOT, move.turnsOfDOT);
            }

            if (move.damage > 0 && multiplier > 1f)
            {
                message.text = "It's super effective on " + t.displayName + "!";
                yield return new WaitForSeconds(turnDelay);
                paused = true;
            }
            else if (move.damage > 0 && multiplier < 1f)
            {
                message.text = "It's not very effective on " + t.displayName + "...";
                yield return new WaitForSeconds(turnDelay);
                paused = true;
            }

            if (t.health.IsDead)
            {
                message.text = t.displayName + " fainted!";
                yield return new WaitForSeconds(turnDelay);
                paused = true;
                if (enemies.Contains(t))
                {
                    RemoveEnemy(t);
                }
            }
        }
        //give more time even if there is no extra message
        if (!paused) 
        {
            yield return new WaitForSeconds(turnDelay);
        }
    }

        List<Entity> GetSupportTargets(Entity user, MoveData move)
        {
            List<Entity> helped = new List<Entity>();

            if (move.isAOE && user != player)
            {
                foreach (Entity enemy in enemies)
                {
                    if (!enemy.health.IsDead) helped.Add(enemy);
                }
            }
            else
            {
                helped.Add(user);
            }
            return helped;
        }
        List<Entity> GetDamageTargets(Entity user,Entity defaultTarget, MoveData move)
        {
            List<Entity> targets = new List<Entity>();

            if (user == player && move.isAOE)
            {
                foreach (Entity enemy in enemies)
                {
                    if (!enemy.health.IsDead) targets.Add(enemy);
                }
            }
            else
            {
                targets.Add(defaultTarget);
            }
            return targets;
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

