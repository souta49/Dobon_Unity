using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlayer : BasePlayer// BasePlayerを継承
{
    // クラスのインスタンス（インスペクターで設定）
    [SerializeField] SelfPlayer _selfPlayer;
    [SerializeField] TextManager _text;

    // 手札の合計数
    int _handTotal;

    // 手札の数字と出せる手札を入れる用とオブジェクトのリスト
    List<int> _hand = new List<int>();
    List<int> _playHand = new List<int>();
    List<GameObject> _object = new List<GameObject>();

    // フラグ
    bool EnemyDobon;

    // シーン開始時に取得
    void OnEnable()
    {
        // イベントハンドラの登録
        EventManager.Instance.OnEnemyTurnStart += HandleEnemyTurnStart;
        EventManager.Instance.OnEnemyDoDrawCard += HandleEnemyDoDrawCard;
        EventManager.Instance.OnEnemyDrawCardAfterDraw += HandleEnemyDrawCardAfterDraw;
        EventManager.Instance.OnEnemyAfterDraw += HandleEnemyAfterDraw;
        EventManager.Instance.OnEnemyTurnEnd += HandleEnemyTurnEnd;
    }

    // シーン終了時に破棄
    void OnDestroy()
    {
        // イベントハンドラの解除
        _event.OnEnemyTurnStart -= HandleEnemyTurnStart;
        _event.OnEnemyDoDrawCard -= HandleEnemyDoDrawCard;
        _event.OnEnemyDrawCardAfterDraw -= HandleEnemyDrawCardAfterDraw;
        _event.OnEnemyAfterDraw -= HandleEnemyAfterDraw;
        _event.OnEnemyTurnEnd -= HandleEnemyTurnEnd;
    }

    // それぞれのイベントのコルーチン開始
    void HandleEnemyTurnStart() => StartCoroutine(TurnStart());
    void HandleEnemyDoDrawCard() => StartCoroutine(DoDrawCard());
    void HandleEnemyDrawCardAfterDraw() => StartCoroutine(DrawCardAfterDraw());
    void HandleEnemyAfterDraw() => StartCoroutine(AfterDraw());
    void HandleEnemyTurnEnd() => StartCoroutine(TurnEnd());

    // オブジェクトをプールに返却
    public void ReturnObject()
    {
        // 全オブジェクト繰り返す
        foreach (GameObject obj in _object)
        {
            // 位置と親オブジェクトをリセット
            obj.transform.localPosition = Vector3.zero;
            obj.transform.SetParent(null);

            // プールに返却
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
                _hand.RemoveAt(i);
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
    }

    // 天保のチェック（オーバーライド）
    public override void Tenho()
    {
        if (_handTotal == _fieldCard._fieldNumber)
        {
            _enemyTenho = true;
        }
        // 親クラスの処理を呼ぶ
        base.Tenho();
    }

    // カードを受け取る処理（オーバーライド）
    public override void ReceiveCard(GameObject obj, int card)
    {
        // オブジェクトとスプライトの取得（親クラス）
        SelfReceive = false;
        base.ReceiveCard(obj, card);

        // ジョーカーなら
        if (card == 52 || card == 53)
        {
            // タグを"Joker"にし、ジョーカー変数を増やす
            cardObj.tag = "Joker";
            _object.Add(cardObj);
            _hand.Add(card); // 表向きに描画するときに必要
            EnemyJoker += 1;
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
            var targetPosition = new Vector3(width + (interval * halfWidth), 2.6f, 0);
            StartCoroutine(_coroutine.MoveCardToPosition(drawnCardObj, targetPosition, 1f));
            width += 1.25f;
        }
    }

    //ドボン返し判定
    public void CanDobonReturn()
    {
        // ドボン返し可能なら勝ち
        if (_handTotal == _fieldCard._fieldNumber)
        {
            _game.EnemyWinner = true;
            _game._bonus *= 2;
        }
        // できなければ負け
        else
        {
            _game.SelfWinner = true;
        }
    }


    /* -------------------------------------------------------------------------------*/
    // 相手のターン開始
    public void StartEnemyTurn()
    {
        // タグをオンにし、テキストをクリア後イベント開始
        SelfPlayerTurn = false;
        _text.PassClearText();
        _event.EnemyTriggerTurnStart();
    }

    // ターンの最初の処理
    IEnumerator TurnStart()
    {
        // 時間調整
        yield return StartCoroutine(_coroutine.delateTime3());

        CanDobon();
        // ドボンできるならする
        if (EnemyDobon)
        {
            // ドボン返しを判定して終了
            EnemyDobon = false;
            _selfPlayer.CanDobonReturn();
            _event.EnemyTriggerTurnEnd();
        }
        // ドローカードならイベントを変更
        else if (DrawCardIndex != 0)
        {
            _event.EnemyTriggerDoDrawCard();
        }
        // それ以外
        else
        {
            CanPlayNomalCard();
            // 出せる手札があるなら出す
            if (_playHand.Count != 0 && _hand.Count != 1)
            {
                yield return StartCoroutine(PlayCard());
                _event.EnemyTriggerTurnEnd();
            }
            // ないならカードを引く
            else
            {
                yield return StartCoroutine(Draw());
                _event.EnemyTriggerAfterDraw();
            }
        }
    }

    // ドローカードインデックスが0じゃないとき
    IEnumerator DoDrawCard()
    {
        CanPlayDrawCard();
        // 出せる手札があるなら出す
        if (_playHand.Count != 0)
        {
            yield return StartCoroutine(PlayCard());
            _event.EnemyTriggerTurnEnd();
        }
        // ないならカードを引く
        else
        {
            yield return StartCoroutine(Draw());
            _event.EnemyTriggerDrawCardAfterDraw();
        }
    }

    // ドローカードを引いた後のターン
    IEnumerator DrawCardAfterDraw()
    {
        // カードが8枚以上なら
        if (_hand.Count >= 8)
        {
            yield return StartCoroutine(Over8Card());
            _event.EnemyTriggerTurnEnd();
        }
        // 7枚以下なら
        else
        {
            CanDobon();
            CanPlayNomalCard();
            // ドボンできるならする
            if (EnemyDobon)
            {
                // _bonusを2倍にし（引きドボン）ドボン返しの判定をしてターン終了
                EnemyDobon = false;
                _game._bonus *= 2;
                _selfPlayer.CanDobonReturn();
                _event.EnemyTriggerTurnEnd();
            }
            // 出せるカードがあるなら出す
            else if (_playHand.Count != 0)
            {
                yield return StartCoroutine(PlayCard());
                _event.EnemyTriggerTurnEnd();
            }
            // ないならカードを引く
            else
            {
                yield return StartCoroutine(Draw());
                _event.EnemyTriggerAfterDraw();
            }
        }
    }

    // カードを引いた後のターン
    IEnumerator AfterDraw()
    {
        // カードが8枚以上なら
        if (_hand.Count >= 8)
        {
            yield return StartCoroutine(Over8Card());
            _event.EnemyTriggerTurnEnd();
        }
        // 7枚以下なら
        else
        {
            CanDobon();
            CanPlayNomalCard();
            // ドボンできるならする
            if (EnemyDobon)
            {
                // _bonusを2倍にし（引きドボン）ドボン返しの判定をしてターン終了
                EnemyDobon = false;
                _game._bonus *= 2;
                _selfPlayer.CanDobonReturn();
            }
            // 出せるカードがあるなら出す
            else if (_playHand.Count != 0)
            {
                yield return StartCoroutine(PlayCard());
            }
            // ないならパスする
            else
            {
                // テキスト表示
                _text.PassNotice();
            }

            // ターンエンドへ
            _event.EnemyTriggerTurnEnd();
        }
    }

    // ターン終了
    IEnumerator TurnEnd()
    {
        // ターン変更
        SelfPlayerTurn = true;
        _game.ChangeTurn();
        yield break;
    }
    /* -------------------------------------------------------------------------------*/


    // ドボンできるかチェック
    void CanDobon()
    {
        // 手札の合計が場の数字と同じか、かつ、相手が出した手札か、かつ4以上か
        if (_handTotal == _fieldCard._fieldNumber && SelfPlayerPlayedCard && _handTotal > 3)
        {
            EnemyDobon = true;
        }
        else
        {
            EnemyDobon = false;
        }
    }

    // ドローカードを持っているかチェック
    void CanPlayDrawCard()
    {
        // 全ての手札をチェック
        for (int i = 0; i < _hand.Count; i++)
        {
            int number = _hand[i] % 13 + 1;

            // 出せるカードをリストに追加
            if (number == 1 || number == 8 || number == 10)
            {
                _playHand.Add(_hand[i]);
            }
        }
    }

    // 出せるカードを持っているかチェック
    void CanPlayNomalCard()
    {
        // 全ての手札をチェック
        for (int i = 0; i < _hand.Count; i++)
        {
            int number = _hand[i] % 13 + 1;
            int suit = _hand[i] / 13;

            // 出せるカードをリストに追加
            if (number == 1 || number == 8 || number == 10 || number == 11 || number == _fieldCard._fieldNumber || suit == _fieldCard._fieldSuit)
            {
                _playHand.Add(_hand[i]);
            }
        }
    }

    // カードを出す処理
    IEnumerator PlayCard()
    {
        // ランダムでカードを出す
        int card = _playHand[Random.Range(0, _playHand.Count)];

        // インデックスの番号とカードナンバー
        int ListIndex = _hand.IndexOf(card);
        int CardNumber = _hand[ListIndex] % 13 + 1;

        // 表向きで表示し、場にカードを出す
        _cardPrefabs.SetCardSprite(_object[ListIndex], card);
        yield return StartCoroutine(_fieldCard.AddCardToBoard(_object[ListIndex], _hand[ListIndex]));

        // フラグを変更し、手札の合計から引く
        SelfPlayerPlayedCard = false;
        _handTotal -= CardNumber;

        // ドローカードならDrawCardIndexに+1追加
        if (CardNumber == 1 || CardNumber == 8 || CardNumber == 10)
        {
            DrawCardIndex += 1;
        }
        // 11ならスートをランダムで選択する
        else if (CardNumber == 11)
        {
            _fieldCard._fieldSuit = Random.Range(0, 4);
        }

        // リストから削除
        _hand.RemoveAt(ListIndex);
        _object.RemoveAt(ListIndex);
        _playHand.Clear();

        // 0枚なら1枚追加
        if (_hand.Count == 0)
        {
            _game.Enemydeal(1);
            yield return StartCoroutine(_game.CheckEnemyJoker());
        }

        // カードポジション修正
        UpdateCardPositions();
    }

    // カードを引く処理
    IEnumerator Draw()
    {
        // ドローインデックスが0でないなら
        if (DrawCardIndex != 0)
        {
            // インデックス分だけカードを引く
            _game.Enemydeal(DrawCardIndex);
            DrawCardIndex = 0;

            // ジョーカーチェック
            yield return StartCoroutine(_game.CheckEnemyJoker());
        }
        // それ以外
        else
        {
            _game.Enemydeal(1);

            // ジョーカーチェック
            yield return StartCoroutine(_game.CheckEnemyJoker());
        }

        // 時間調整
        yield return StartCoroutine(_coroutine.delateTime2());
    }

    // カードが8枚以上の処理
    IEnumerator Over8Card()
    {
        // フラグ変更
        _game.SelfWinner = true;
        _game.Over8Card = true;

        // 時間調整
        yield return StartCoroutine(_coroutine.delateTime2());
    }

    // カードをオープンする処理
    public void OpenCard()
    {
        // 手札の数だけ繰り返す
        for (int i = 0; i < _object.Count; i++)
        {
            // 辞書からオブジェクトのスプライトレンダラー取得
            var spriteRenderer = _cardPrefabs.spriteRendererCache[_object[i]];

            // 現在の裏向きのスプライトを取得
            Sprite backSprite = spriteRenderer.sprite;

            // 表向きに変更
            _cardPrefabs.SetCardSprite(_object[i], _hand[i]);

            // 表のスプライトを取得
            Sprite frontSprite = spriteRenderer.sprite;

            // スプライトレンダラーと表、裏のスプライトを渡してコルーチン開始
            StartCoroutine(_coroutine.FlipCard(spriteRenderer, frontSprite, backSprite));
        }
    }

    // ジョーカーをオープンする処理
    public IEnumerator OpenJoker()
    {
        // オブジェクトリストから"Joker"タグを探す（リストの後ろから）
        for (int i = _object.Count - 1; i >= 0; i--)
        {
            if (_object[i].CompareTag("Joker"))
            {
                // 辞書からオブジェクトのスプライトレンダラー取得
                var spriteRenderer = _cardPrefabs.spriteRendererCache[_object[i]];

                // 現在の裏向きのスプライトを取得
                Sprite backSprite = spriteRenderer.sprite;

                // 表向きに変更
                _cardPrefabs.SetCardSprite(_object[i], _hand[i]);

                // 表のスプライトを取得
                Sprite frontSprite = spriteRenderer.sprite;

                // スプライトレンダラーと表、裏のスプライトを渡してコルーチン開始
                yield return StartCoroutine(_coroutine.FlipCard(spriteRenderer, frontSprite, backSprite));
            }
        }
    }
}
