using System;

public class EventManager : SingletonMonoBehaviour<EventManager> // シングルトン
{
    /* イベント変数は読み取り専用の特性を持つ */

    // ゲーム進行制御イベント宣言
    public event Action OnInitialize;
    public event Action OnPlayerTurn;
    public event Action OnEnemyTurn;
    public event Action OnCalcPoint;
    public event Action OnNotCount;
    public event Action OnResult;

    // SelfPlayer制御イベント宣言
    public event Action OnSelfTurnStart;
    public event Action OnSelfDoDrawCard;
    public event Action OnSelfDrawCardAfterDraw;
    public event Action OnSelfAfterDraw;
    public event Action OnSelfTurnEnd;

    // EnemyPlayer制御イベント宣言
    public event Action OnEnemyTurnStart;
    public event Action OnEnemyDoDrawCard;
    public event Action OnEnemyDrawCardAfterDraw;
    public event Action OnEnemyAfterDraw;
    public event Action OnEnemyTurnEnd;


    // <summary> ゲーム進行制御イベントトリガー（発生）</summary>
    public void TriggerInitialize() => OnInitialize?.Invoke();
    public void TriggerPlayerTurn() => OnPlayerTurn?.Invoke();
    public void TriggerEnemyTurn() => OnEnemyTurn?.Invoke();
    public void TriggerCalcPoint() => OnCalcPoint?.Invoke();
    public void TriggerNotCount() => OnNotCount?.Invoke();
    public void TriggerResult() => OnResult?.Invoke();

    // <summary> SelfPlayer制御イベントトリガー（発生）</summary>
    public void SelfTriggerTurnStart() => OnSelfTurnStart?.Invoke();
    public void SelfTriggerDoDrawCard() => OnSelfDoDrawCard?.Invoke();
    public void SelfTriggerDrawCardAfterDraw() => OnSelfDrawCardAfterDraw?.Invoke();
    public void SelfTriggerAfterDraw() => OnSelfAfterDraw?.Invoke();
    public void SelfTriggerTurnEnd() => OnSelfTurnEnd?.Invoke();

    // <summary> EnemyPlayer制御イベントトリガー（発生）</summary>
    public void EnemyTriggerTurnStart() => OnEnemyTurnStart?.Invoke();
    public void EnemyTriggerDoDrawCard() => OnEnemyDoDrawCard?.Invoke();
    public void EnemyTriggerDrawCardAfterDraw() => OnEnemyDrawCardAfterDraw?.Invoke();
    public void EnemyTriggerAfterDraw() => OnEnemyAfterDraw?.Invoke();
    public void EnemyTriggerTurnEnd() => OnEnemyTurnEnd?.Invoke();
}
