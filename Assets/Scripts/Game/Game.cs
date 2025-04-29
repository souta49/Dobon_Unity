using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class Game : SingletonMonoBehaviour<Game>// シングルトンクラス
{
    // 各クラスのインスタンス（インスペクターで設定）
    [SerializeField] SelfPlayer     _selfPlayer;
    [SerializeField] EnemyPlayer    _enemyPlayer;
    [SerializeField] Deck           _deck;
    [SerializeField] Dora           _dora;
    [SerializeField] FieldCard      _fieldCard;
    [SerializeField] TextManager    _text;

    // シングルトンクラスのインスタンス
    EventManager        _event;
    CoroutineManager    _coroutine;

    // 変数
    static int  _selfPoint = 30;
    static int  _enemyPoint = 30;
    public int  _bonus = 1;
    int         card;
    GameObject  obj;

    // プロパティ
    public static int SelfPoint { get { return _selfPoint; } }
    public static int EnemyPoint { get { return _enemyPoint; } }

    // フラグ
    public bool SelfWinner;
    public bool EnemyWinner;
    public bool CheckTenho;
    public bool Over8Card;
    public bool CheckNotCount;
    public static bool SelfWin;
    /* staticでも実装できそう。シングルトン変数の方が変更がしやすいっぽい。*/

    // シーン開始時に取得
    void OnEnable()
    {
        // イベントハンドラの登録
        EventManager.Instance.OnInitialize += HandleInitialize;
        EventManager.Instance.OnPlayerTurn += HandlePlayerTurn; 
        EventManager.Instance.OnEnemyTurn += HandleEnemyTurn; 
        EventManager.Instance.OnCalcPoint += HandleCalcPoint; 
        EventManager.Instance.OnNotCount += HandleNotCount; 
        EventManager.Instance.OnResult += HandleResult;
    }

    // シーン終了時に破棄
    void OnDestroy()
    {
        // イベントハンドラの解除
        _event.OnInitialize -= HandleInitialize;
        _event.OnPlayerTurn -= HandlePlayerTurn;
        _event.OnEnemyTurn -= HandleEnemyTurn;
        _event.OnCalcPoint -= HandleCalcPoint;
        _event.OnNotCount -= HandleNotCount;
        _event.OnResult -= HandleResult;
    }

    void Start()
    {
        // ポイントリセット
        _selfPoint = 30;
        _enemyPoint = 30;
        
        // シングルトンのインスタンス取得
        _event          = EventManager.Instance;
        _coroutine      = CoroutineManager.Instance;
        
        // 初期化スタート
        _event.TriggerInitialize();
    }

    // それぞれのイベントのコルーチン開始
    void HandleInitialize() => StartCoroutine(Initialize()); 
    void HandlePlayerTurn() => StartCoroutine(PlayerTurn());
    void HandleEnemyTurn() => StartCoroutine(EnemyTurn()); 
    void HandleCalcPoint() => StartCoroutine(CalcPoint()); 
    void HandleNotCount() => StartCoroutine(NotCount()); 
    void HandleResult() => StartCoroutine(Result());


    /* ----------------------------------------------------------------------*/
    // 初期化
    IEnumerator Initialize()
    {
        // 初期化処理
        OllInitialize();
        yield return StartCoroutine(DealCards());
        _selfPlayer.Tenho();
        _enemyPlayer.Tenho();

        // 二人とも天保だったら
        if (CheckNotCount)
        {
            // ノーカン
            _event.TriggerNotCount();
        }
        // どちらかが天保なら
        else if (SelfWinner || EnemyWinner)
        {
            // 時間調整
            yield return StartCoroutine(_coroutine.delateTime2());
            // 得点計算へ
            _event.TriggerCalcPoint();
        }
        else
        {
            // ターンをランダムで選択
            bool FirstTurn = UnityEngine.Random.value > 0.5f;
            if (FirstTurn)
            {
                // 引き天保用
                BasePlayer.SelfPlayerPlayedCard = false;

                // 自分のターンへ
                _event.TriggerPlayerTurn();
            }
            else
            {
                // 引き天保用
                BasePlayer.SelfPlayerPlayedCard = true;

                // 相手のターンへ
                _event.TriggerEnemyTurn();
            }
        }
        yield break;
    }

    // 自分のターン
    IEnumerator PlayerTurn()
    {
        // プレイヤー処理スタート
        _selfPlayer.StartPlayerTurn();
        yield break;
    }

    // ターン変更
    public void ChangeTurn()
    {
        // どちらかがドボンしたら
        if (SelfWinner || EnemyWinner)
        {
            // 得点計算へ
            _event.TriggerCalcPoint();
        }
        // フラグで管理
        else if (BasePlayer.SelfPlayerTurn)
        {
            // 自分のターンへ
            _event.TriggerPlayerTurn();
        }
        else
        {
            // 相手のターンへ
            _event.TriggerEnemyTurn();
        }
    }
    
    // 相手のターン
    IEnumerator EnemyTurn()
    {
        // 相手の処理スタート
        _enemyPlayer.StartEnemyTurn();
        yield break;
    }

    // 得点計算ターン
    IEnumerator CalcPoint()
    {
        // 相手のカードをオープンし、どちらがドボンしたか表示
        _enemyPlayer.OpenCard();
        _text.ShowDobonText();

        // 時間調整
        yield return StartCoroutine(_coroutine.delateTime3());

        // ドラをオープンし、得点が倍になるかチェック
        _dora.OpenDora();
        _dora.CheakDora();

        // 時間調整
        yield return StartCoroutine(_coroutine.delateTime3());

        // 得点計算メソッド
        CalculatePoint();

        // プレイヤー負け
        if (_selfPoint <= 0)
        {
            // CPU勝ちで結果画面へ
            SelfWin = false;
            _event.TriggerResult();
        }
        // CPU負け
        else if (_enemyPoint <= 0)
        {
            // プレイヤー勝ちで結果画面へ
            SelfWin = true;
            _event.TriggerResult();
        }
        // それ以外は次のゲームへ
        else
        {
            // オブジェクトをリセット
            OllObjectReturn();

            // 初期化へ
            _event.TriggerInitialize();
        }
    }

    // ノーカン
    IEnumerator NotCount()
    {
        CheckNotCount = false;
        // やり直し
        _event.TriggerInitialize();
        yield break;
    }

    // 結果ターン
    IEnumerator Result()
    {
        // 1フレーム待ち結果を取得
        yield return null;

        // Resultシーンへ
        SceneManager.LoadScene("Result");
        yield break;
    }
    /* ----------------------------------------------------------------------------- */


    // オブジェクトをプールにすべて返却
    void OllObjectReturn()
    {
        _selfPlayer.ReturnObject();
        _enemyPlayer.ReturnObject();
        _deck.ReturnObject();
        _fieldCard.ReturnObject();
        _dora.ReturnObject();
    }

    // 手札、山札、場の札、ドラ、テキストを初期化
    void OllInitialize()
    {
        _selfPlayer.Initialize();
        _enemyPlayer.Initialize();
        _deck.InitializeDeck();
        _fieldCard.ClearBoard();
        _dora.ClearDora();
        _text.DobonClearText();
    }

    // ゲーム開始時にカードを配る処理
    IEnumerator DealCards()
    {
        // 3枚ずつ配る
        Selfdeal(3);
        Enemydeal(3);

        // 最初の場の札を配置する
        FieldDeploy();

        // ドラを配置する
        (obj, card) = _deck.DrawCard();
        CardPrefabs.Instance.SetDoraSprite(obj);
        StartCoroutine(_dora.ReceiveDora(obj, card));

        // ジョーカーのチェック
        yield return StartCoroutine(CheckSelfJoker());
        yield return StartCoroutine(CheckEnemyJoker());

        // 最初の場の札がジョーカーだったら繰り返す
        while (_fieldCard._fieldSuit == 4)
        {
            // レートを2倍し、ジョーカー枚数を減らし、時間調整
            _bonus *= 2;
            BasePlayer.OllJoker -= 1;
            _fieldCard.boardCards.Add(card);
            yield return StartCoroutine(_coroutine.delateTime3());

            // 場の札を再び配置する
            FieldDeploy();
        }
    }

    // 場の札にカードを配置する処理
    void FieldDeploy()
    {
        // 山札からカードを引き、場の札に配置する
        (obj, card) = _deck.DrawCard();
        CardPrefabs.Instance.SetCardSprite(obj, card);
        StartCoroutine(_fieldCard.AddCardToBoard(obj, card));
    }

    // プレイヤーのジョーカーチェック
    public IEnumerator CheckSelfJoker()
    {
        // プレイヤーの手札にジョーカーがあったら繰り返す（引いたカードにジョーカーがあったとき用に繰り返している）
        while (BasePlayer.SelfJoker != 0)
        {
            // コルーチン中にボタンを押せないようにオフにする
            BaseUI.Instance._canDraw = false;

            // ジョーカーの枚数を代入し、変数を0に
            int Value = BasePlayer.SelfJoker;
            BasePlayer.SelfJoker = 0;

            // ジョーカーの処理へ
            yield return StartCoroutine(HandleJoker(Value, _selfPlayer.DelateJoker, Selfdeal));
        }
    }

    // CPUのジョーカーチェック
    public IEnumerator CheckEnemyJoker()
    {
        // CPUの手札にジョーカーがあったら繰り返す（引いたカードにジョーカーがあったとき用に繰り返している）
        while (BasePlayer.EnemyJoker != 0)
        {
            // 時間調整をし、ジョーカーをオープン
            yield return StartCoroutine(_coroutine.delateTime3());
            StartCoroutine(_enemyPlayer.OpenJoker());

            // ジョーカーの枚数を代入し、変数を0に
            int Value = BasePlayer.EnemyJoker;
            BasePlayer.EnemyJoker = 0;

            // ジョーカーの処理へ
            yield return StartCoroutine(HandleJoker(Value, _enemyPlayer.DelateJoker, Enemydeal));
        }
    }

    // 手札にジョーカーがあったときの処理（ジョーカーの枚数、ジョーカー削除処理、カードを引く処理）
    IEnumerator HandleJoker(int jokerValue, Func<IEnumerator> delateJokerCoroutine, Action<int> dealMethod)
    {
        // 時間調整をし、ジョーカーをプールに返却後、、カードを配る
        yield return StartCoroutine(_coroutine.delateTime3());
        yield return StartCoroutine(delateJokerCoroutine());
        dealMethod(jokerValue * 2);
    }

    // プレイヤーにカードを配る処理
    public void Selfdeal (int number)
    {
        // 指定枚数配る
        for (int i = 0; i < number; i++)
        {
            // オブジェクトと数字を受け取る
            (obj, card) = _deck.DrawCard();
            _selfPlayer.ReceiveCard(obj, card);
        }
    }

    // CPUにカードを配る処理
    public void Enemydeal(int number)
    {
        // 指定枚数配る
        for (int i = 0; i < number; i++)
        {
            // オブジェクトと数字を受け取る
            (obj, card) = _deck.DrawCard();
            _enemyPlayer.ReceiveCard(obj, card);
        }
    }

    // 得点計算
    void CalculatePoint()
    {
        // 自分がドボン
        if (SelfWinner)
        {
            // 参照渡し
            UpdatePoints(ref _selfPoint, ref _enemyPoint);
            SelfWinner = false;
        }
        // 相手がドボン
        else if (EnemyWinner)
        {
            // 参照渡し
            UpdatePoints(ref _enemyPoint, ref _selfPoint);
            EnemyWinner = false;
        }
    }

    // 得点加算（第1引数が勝った方、第2引数が負けたほう）
    void UpdatePoints(ref int winnerPoints, ref int loserPoints)
    {
        // 天保か否か
        if (CheckTenho)
        {
            // 20点×レート
            winnerPoints += 20 * _bonus;
            loserPoints -= 20 * _bonus;
            CheckTenho = false;
        }
        // カードが８枚以上になった場合
        else if (Over8Card)
        {
            // 15点×レート
            winnerPoints += 15 * _bonus;
            loserPoints -= 15 * _bonus;
            Over8Card = false;
        }
        else
        {
            // 場の数×レート
            winnerPoints += _fieldCard._fieldNumber * _bonus;
            loserPoints -= _fieldCard._fieldNumber * _bonus;
        }

        // レートリセット
        _bonus = 1;
    }
}