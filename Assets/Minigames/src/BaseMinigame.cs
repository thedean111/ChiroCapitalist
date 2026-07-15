using System;
using UnityEngine;

public class BaseMinigame : MonoBehaviour
{
    public Action onMinigameEnd;
    public StatCategory mainStat;

    protected bool gameStarted = false; /* This flag identifies if the player has put in the first action for the game. When this happens the actual update loop
    for the game should start. */
    protected NPCStats stats; /* The stats of the current NPC being adjusted */
    protected bool _active = false;

    private float _score;
    public float score {
        get { return _score; }
        set { _score = Mathf.Clamp01(value); }
    } /* Every minigame awards the player based on the score. It will always be between [0,1] where 0 is no reward and 1 is full reward */

    /// <summary>
    /// Handles how a minigame should be started. Children of this class will likely have more logic for setting up custom components.
    /// </summary>
    public virtual void OpenGame(NPCStats _stats) {
        _active = true;
        gameStarted = false;
        stats = _stats;
        score = 1;
    }

    /// <summary>
    /// General behavior for completing a game. Currently just invokes the listeners of the end action.
    /// Children of this class will likely have custom logic for animations, effects, and other deconstruction.
    /// </summary>
    public virtual void CompleteGame() {
        Debug.Log("Ending " + this.GetType() + " with a score of: " + score);
        _active = false;
        onMinigameEnd?.Invoke();
    }

    public virtual void OnClick() {}
}
