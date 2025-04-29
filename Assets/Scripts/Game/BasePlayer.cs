using UnityEngine;

public class BasePlayer : MonoBehaviour
{
    // 各クラスのインスタンス（インスペクターで設定）
    [SerializeField] protected FieldCard    _fieldCard;
    [SerializeField] protected Deck         _deck;

    // シングルトンクラスのインスタンス
    protected CardPrefabs        _cardPrefabs;
    protected Game               _game;
    protected BaseUI             _baseUI;
    protected CoroutineManager   _coroutine;
    protected EventManager       _event;

    // フラグ
    protected static bool _selfTenho;
    protected static bool _enemyTenho;
    protected static bool SelfReceive;
    public static bool SelfPlayerPlayedCard;
    public static bool SelfPlayerTurn;

    // 変数
    protected static int DrawCardIndex = 0;
    public static int SelfJoker = 0;
    public static int EnemyJoker = 0;
    public static int OllJoker = 2;

    // カードを受け取る用
    protected GameObject cardObj;

    void Start()
    {
        // シングルトンクラスのインスタンス取得
        _cardPrefabs = CardPrefabs.Instance;
        _game        = Game.Instance;
        _baseUI      = BaseUI.Instance;
        _coroutine   = CoroutineManager.Instance;
        _event       = EventManager.Instance;
    }

    // カードを受け取る処理（オーバーライドされてる）
    public virtual void ReceiveCard(GameObject obj, int card)
    {
        // オブジェクトを受け取る
        cardObj = obj;

        // 自分か相手かで裏表の描画を変更
        if (SelfReceive)
        {
            _cardPrefabs.SetCardSprite(cardObj, card);
        }
        else
        {
            _cardPrefabs.SetBackSprite(cardObj);
        }
    }

    // 天保チェック（オーバーライドされてる）
    public virtual void Tenho()
    {
        //両方天保
        if (_selfTenho && _enemyTenho)
        {
            _selfTenho = false;
            _enemyTenho = false;
            _game.CheckNotCount = true;
        }
        // 自分が天保
        else if (_selfTenho)
        {
            _selfTenho = false;
            _game.SelfWinner = true;
            _game.CheckTenho = true;
        }
        // 相手が天保
        else if (_enemyTenho)
        {
            _enemyTenho = false;
            _game.EnemyWinner = true;
            _game.CheckTenho = true;
        }
    }
}