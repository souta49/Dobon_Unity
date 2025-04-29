using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfPlayer : BasePlayer// BasePlayerを継承
{
    // クラスのインスタンス（インスペクターで設定）
    [SerializeField] EnemyPlayer _enemyPlayer;

    // 手札の合計数（参照のみ）
    public int _handTotal { get; private set; }

    // 手札の数字とオブジェクトのリスト
    List<int>           _hand = new List<int>();
    List<GameObject>    _object = new List<GameObject>();

    // クリックされたオブジェクトを受け取る用とカード受け取る用のデリゲート
    public static GameObject ClickObject;
    delegate (GameObject, int) DrawCardDelegate();

    // シーン開始時に取得
    void OnEnable()
    {
        // イベントハンドラの登録
        EventManager.Instance.OnSelfTurnStart += HandleSelfTurnStart; 
        EventManager.Instance.OnSelfDoDrawCard += HandleSelfDoDrawCard; 
        EventManager.Instance.OnSelfDrawCardAfterDraw += HandleSelfDrawCardAfterDraw; 
        EventManager.Instance.OnSelfAfterDraw += HandleSelfAfterDraw; 
        EventManager.Instance.OnSelfTurnEnd += HandleSelfTurnEnd;
    }

    // シーン終了時に破棄
    void OnDestroy()
    {
        // イベントハンドラの解除
        _event.OnSelfTurnStart -= HandleSelfTurnStart; 
        _event.OnSelfDoDrawCard -= HandleSelfDoDrawCard; 
        _event.OnSelfDrawCardAfterDraw -= HandleSelfDrawCardAfterDraw;
        _event.OnSelfAfterDraw -= HandleSelfAfterDraw; 
        _event.OnSelfTurnEnd -= HandleSelfTurnEnd;
    }

    // それぞれのイベントのコルーチン開始
    void HandleSelfTurnStart() => StartCoroutine(TurnStart()); 
    void HandleSelfDoDrawCard() => StartCoroutine(DoDrawCard()); 
    void HandleSelfDrawCardAfterDraw() => StartCoroutine(DrawCardAfterDraw()); 
    void HandleSelfAfterDraw() => StartCoroutine(AfterDraw()); 
    void HandleSelfTurnEnd() => StartCoroutine(TurnEnd());

    // オブジェクトをプールに返却
    public void ReturnObject()
    {
        // 全オブジェクト繰り返す
        foreach (GameObject obj in _object)
        {
            // 位置と親オブジェクトの設定とタグをリセット
            obj.transform.localPosition = Vector3.zero;
            obj.transform.SetParent(null);
            obj.tag = "Untagged";

            // 明るさをリセットしプールに返却
            _cardPrefabs.ResetWhiteColor(obj);
            _cardPrefabs.ReturnCardObject(obj);
        }
    }

    // 手札からジョーカーを削除する処理
    public IEnumerator DelateJoker()
    {
        // オブジェクトリストから"Joker"タグを探し、プールに返却する
        for (int i = _object.Count - 1; i >= 0; i--)
        {
            if (_object[i].CompareTag("Joker"))
            {
                // タグを戻し、返却、リストから削除
                _object[i].tag = "Untagged";
                _cardPrefabs.ReturnCardObject(_object[i]);
                _object.RemoveAt(i);
            }
        }
        yield break;
    }

    // 初期化
    public void Initialize()
    {
        _object.Clear();
        _hand.Clear();
        _handTotal = 0;

        // 親の変数も初期化
        DrawCardIndex = 0;
        OllJoker = 2;
    }

    // 天保のチェック（オーバーライド）
    public override void Tenho()
    {
        if (_handTotal == _fieldCard._fieldNumber)
        {
            _selfTenho = true;
        }
    }

    // カードを受け取る処理（オーバーライド）
    public override void ReceiveCard(GameObject obj, int card)
    {
        // オブジェクトとスプライト取得（親クラス）
        SelfReceive = true;
        base.ReceiveCard(obj, card);

        // ジョーカーなら
        if (card == 52 || card == 53)
        {
            // タグを"Joker"にし、ジョーカー変数を増やす
            cardObj.tag = "Joker";
            _object.Add(cardObj);
            SelfJoker += 1;
            OllJoker -= 1;
            _fieldCard.boardCards.Add(card);
        }
        // ジョーカー以外なら
        else
        {
            // 手札に追加
            _hand.Add(card);
            _handTotal += card % 13 + 1;
            _object.Add(cardObj);

            // 各種設定
            cardObj.name = card.ToString();
            cardObj.transform.SetParent(transform);
            cardObj.tag = "Self";
        }

        // カードの位置更新
        UpdateCardPositions();
    }

    // カードのポジション設定
    void UpdateCardPositions()
    {
        var interval = _object.Count - 1;
        var halfWidth = -0.625f;
        var width = -3.0f;

        // 全部のカード移動（1秒で）
        foreach (var drawnCardObj in _object)
        {
            var targetPosition = new Vector3(width + (interval * halfWidth), -2.2f, 0);
            StartCoroutine(_coroutine.MoveCardToPosition(drawnCardObj, targetPosition, 1f)); 
            width += 1.25f;
        }
    }

    // ドボン返し判定
    public void CanDobonReturn()
    {
        // ドボン返し可能なら勝ち
        if (_handTotal == _fieldCard._fieldNumber)
        {
            _game.SelfWinner = true;
            _game._bonus *= 2;
        }
        // できなければ負け
        else
        {
            _game.EnemyWinner = true;
        }
    }


    /* ----------------------------------------------------------------------------- */
    // プレイヤーターンの開始
    public void StartPlayerTurn()
    {
        // タグをオンにし、イベント開始
        SelfPlayerTurn = true;
        _event.SelfTriggerTurnStart();
    }

    // ターンの最初の処理
    IEnumerator TurnStart()
    {
        CanDobon();
        // ドローカードならイベントを変更
        if (DrawCardIndex != 0)
        {
            _event.SelfTriggerDoDrawCard();
        }
        // それ以外はプレイヤーの行動を要求
        else
        {
            CanPlayNomalCard();
            yield return StartCoroutine(WaitForStart());
        }
    }

    // ドローカードインデックスが0じゃないとき
    IEnumerator DoDrawCard()
    {
        // ドローカードがあるか確認し、行動を要求
        CanPlayDrawCard();
        yield return StartCoroutine(WaitForDoDrawcard());
    }

    // ドローカードを引いた後のターン
    IEnumerator DrawCardAfterDraw()
    {
        // カード8枚以上なら
        if (_hand.Count >= 8)
        {
            // カードクリックをオフにするためにここでターンを変え、ボタンをオフにする
            SelfPlayerTurn = false;
            ButtonOf();

            // 8枚の処理を行いターンエンドへ
            yield return StartCoroutine(Over8Card());
            _event.SelfTriggerTurnEnd();
        }
        // 7枚以下ならプレイヤーの行動を要求
        else
        {
            _baseUI._canDraw = true;
            CanDobon();
            CanPlayNomalCard();
            yield return StartCoroutine(WaitForDrawCardAfterDraw());
        }
    }

    // カードを引いた後のターン
    IEnumerator AfterDraw()
    {
        // カードが8枚以上なら
        if (_hand.Count >= 8)
        {
            // カードクリックをオフにするためにここでターンを変え、ボタンをオフにする
            SelfPlayerTurn = false;
            ButtonOf();

            // 8枚の処理を行いターンエンドへ
            yield return StartCoroutine(Over8Card());
            _event.SelfTriggerTurnEnd();
        }
        // 7枚以下ならプレイヤーの行動を要求
        else
        {
            CanDobon();
            CanPlayNomalCard();
            _baseUI._canDraw = false;
            _baseUI._canPass = true;
            yield return StartCoroutine(WaitForAfterDraw());
        }
    }

    // ターン終了
    IEnumerator TurnEnd()
    {
        // ボタンをオフに
        _baseUI._canDraw = false;
        _baseUI._canPass = false;

        // ターン変更
        SelfPlayerTurn = false;
        _game.ChangeTurn();
        yield break;
    }
    /* -------------------------------------------------------------------------------*/


    // ドボンできるかチェック
    void CanDobon()
    {
        // 手札の合計が場の数字と同じか、かつ、相手が出した手札か
        if (_handTotal == _fieldCard._fieldNumber && !SelfPlayerPlayedCard)
        {
            _baseUI._canDobon = true;
        }
        else
        {
            _baseUI._canDobon = false;
        }
    }

    // ドローカードを持っているかチェック
    void CanPlayDrawCard()
    {
        // ドローフラグはオン
        _baseUI._canDraw = true;

        // 全ての手札をチェック
        for (int i = 0; i < _hand.Count; i++)
        {
            int number = _hand[i] % 13 + 1;
            if (number != 1 && number != 8 && number != 10)
            {
                // ドローカード以外は暗くする
                var spriteRenderer = _cardPrefabs.spriteRendererCache[_object[i]];
                spriteRenderer.color = Color.gray;
            }
            else
            {
                // ドローカードは明るくする
                var spriteRenderer = _cardPrefabs.spriteRendererCache[_object[i]];
                spriteRenderer.color = Color.white;
            }
        }
    }

    // 出せるカードを持っているかチェック
    void CanPlayNomalCard()
    {
        // ドローフラグはオン
        _baseUI._canDraw = true;

        // 全ての手札をチェック
        for (int i = 0; i < _hand.Count; i++)
        {
            int number = _hand[i] % 13 + 1;
            int suit = _hand[i] / 13;
            if (number != 1 && number != 8 && number != 10 && number != 11 && number != _fieldCard._fieldNumber && suit != _fieldCard._fieldSuit)
            {
                // 出せないカードは暗くする
                var spriteRenderer = _cardPrefabs.spriteRendererCache[_object[i]];
                spriteRenderer.color = Color.gray;
            }
            else
            {
                // 出せるカードは明るくする
                var spriteRenderer = _cardPrefabs.spriteRendererCache[_object[i]];
                spriteRenderer.color = Color.white;
            }
        }
    }

    // カードがクリックされた処理
    IEnumerator OnClickCard(GameObject obj)
    {
        // インデックスの番号とカードのナンバー
        int ListIndex = _object.IndexOf(obj);
        int CardNumber = _hand[ListIndex] % 13 + 1;

        _object[ListIndex].tag = "Untagged";

        //場にカードを出す（コルーチン）
        StartCoroutine(_fieldCard.AddCardToBoard(obj, _hand[ListIndex]));

        // フラグを変更し、手札の合計から引く
        SelfPlayerPlayedCard = true;
        _handTotal -= CardNumber;

        // リストから削除
        _hand.RemoveAt(ListIndex);
        _object.RemoveAt(ListIndex);

        // ドローカードならDrawCardIndexに+1追加
        if (CardNumber == 1 || CardNumber == 8 || CardNumber == 10)
        {
            DrawCardIndex += 1;
        }
        // 11ならスートを選択する
        else if (CardNumber == 11)
        {
            // ボタンとカードを押せないようにする
            ButtonOf();
            ChangeColor();

            // ボタンを表示し、スートの入力待ちをスタート
            _baseUI._canSuite = true;
            yield return StartCoroutine(SuitInput());
        }

        // 0枚なら1枚追加
        if (_hand.Count == 0)
        {
            _game.Selfdeal(1);
            yield return StartCoroutine(_game.CheckSelfJoker());
        }

        // カードポジション修正
        UpdateCardPositions();

        // イベント変更
        _event.SelfTriggerTurnEnd();
    }

    // ボタンをオフにする処理
    void ButtonOf()
    {
        _baseUI._canDobon = false;
        _baseUI._canDraw = false;
        _baseUI._canPass = false;
    }

    // オブジェクトのカラーをグレーにする
    void ChangeColor()
    {   
        foreach (GameObject obj in _object)
        {
            _cardPrefabs.ResetGrayColor(obj);
        }
    }

    // スートの入力待ち処理
    IEnumerator SuitInput()
    {
        // ボタンの入力を待つ
        yield return new WaitUntil(() => _baseUI.PressedButton != 0);

        // どのボタンが押されたかを検知し、それぞれのスートを代入
        switch (_baseUI.PressedButton)
        {
            case 1:
                _fieldCard._fieldSuit = 1;
                break;

            case 2:
                _fieldCard._fieldSuit = 2;
                break;

            case 3:
                _fieldCard._fieldSuit = 0;
                break;

            case 4:
                _fieldCard._fieldSuit = 3;
                break;
        }

        // ボタンとリスナー用変数をオフに
        _baseUI.PressedButton = 0;
        _baseUI._canSuite = false;
    }

    // カードが8枚以上の処理
    IEnumerator Over8Card()
    {
        // フラグ変更
        _game.EnemyWinner = true;
        _game.Over8Card = true;

        // 時間調整
        yield return StartCoroutine(_coroutine.delateTime2());
    }
    
    // マウスのフラグをオンに
    IEnumerator MouseDown()
    {
        _baseUI._onMouseDown = true;
        yield break;
    }


    /// <summary>
    /// 以下、4パターンのコルーチン（プレイヤーの入力待ち）
    /// </summary>

    // 入力された後の処理まとめ
    IEnumerator HandlePlayerInput(DrawCardDelegate drawCardMethod, Action afterDobonAction)
    {
        // マウスのフラグをオフに
        _baseUI._onMouseDown = false;

        // ドローボタンが押されたとき
        if (_baseUI.isDrawButtonClicked)
        {
            _baseUI.isDrawButtonClicked = false;

            // ドローインデックスが0でないなら
            if (DrawCardIndex != 0)
            {
                // インデックス分だけカードを引く
                _game.Selfdeal(DrawCardIndex);
                DrawCardIndex = 0;

                // ジョーカーチェック
                yield return StartCoroutine(_game.CheckSelfJoker());

                // イベント変更
                _event.SelfTriggerDrawCardAfterDraw();
            }
            // その他はカードを1枚引く
            else
            {
                _game.Selfdeal(1);

                // ジョーカーチェック
                yield return StartCoroutine(_game.CheckSelfJoker());

                // イベント変更
                _event.SelfTriggerAfterDraw();
            }
        }
        // ドボンボタンが押されたとき
        else if (_baseUI.isDobonButtonClicked)
        {
            _baseUI.isDobonButtonClicked = false;

            // 引きドボンかとドボン返しされるか確認
            afterDobonAction();
            _enemyPlayer.CanDobonReturn();

            // イベント変更
            _event.SelfTriggerTurnEnd();
        }
        // パスボタンが押されたとき
        else if (_baseUI.isPassButtonClicked)
        {
            _baseUI.isPassButtonClicked = false; 

            // イベント変更
            _event.SelfTriggerTurnEnd();
        }
        // カードがクリックされたとき（カードを出したとき）
        else if (_baseUI._onCardClick)
        {
            _baseUI._onCardClick = false;

            // カードを出す処理
            StartCoroutine(OnClickCard(ClickObject));
        }
    }

    // 入力待ちの処理
    // 第一引数：カードを引いたときのオブジェクトと数字、第二引数：引きドボンかの判定、第三引数：入力の判定
    IEnumerator WaitForInput(DrawCardDelegate drawCardMethod, Action afterDobonAction, Func<bool> waitCondition)
    {
        // コルーチン中にマウスを押せないように調整
        yield return StartCoroutine(MouseDown());
        // 入力があるまで待機（非同期）
        yield return new WaitUntil(waitCondition);
        // 入力後の処理へ
        StartCoroutine(HandlePlayerInput(drawCardMethod, afterDobonAction));
    }

    // スタートターンのコルーチンの引数設定
    IEnumerator WaitForStart()
    {
        // 入力待ちへ処理を渡す
        yield return WaitForInput(
            () => _deck.DrawCard(),  // カードを引く処理
            () => { },               // 引きドボンはない
            () => _baseUI.isDrawButtonClicked || _baseUI.isDobonButtonClicked || _baseUI._onCardClick  //Pass以外のボタン
        );
    }

    // ドローカードインデックスが0じゃないときのコルーチンの引数設定
    IEnumerator WaitForDoDrawcard()
    {
        // 入力待ちへ処理を渡す
        yield return WaitForInput(
            () =>_deck.DrawCard(),  // カードを引く処理
            () => { },              // 引きドボンはない
            () => _baseUI.isDrawButtonClicked || _baseUI.isDobonButtonClicked || _baseUI._onCardClick  // Pass以外のボタン
        );
    }

    // ドローカードを引いた後のコルーチンの引数設定
    IEnumerator WaitForDrawCardAfterDraw()
    {
        // 入力待ちへ処理を渡す
        yield return WaitForInput(
            () => _deck.DrawCard(),  // カードを引く処理
            () => _game._bonus *= 2, // 引きドボンで2倍
            () => _baseUI.isDrawButtonClicked || _baseUI.isDobonButtonClicked || _baseUI._onCardClick  //Pass以外のボタン
        );
    }

    // カードを引いた後のコルーチンの引数設定
    IEnumerator WaitForAfterDraw()
    {
        // 入力待ちへ処理を渡す
        yield return WaitForInput(
            () => { return (null,0); },   // カードは引けない
            () => _game._bonus *= 2,      // 引きドボンで2倍
            () => _baseUI.isPassButtonClicked || _baseUI.isDobonButtonClicked || _baseUI._onCardClick  // draw以外のボタン
        );
    }
}
