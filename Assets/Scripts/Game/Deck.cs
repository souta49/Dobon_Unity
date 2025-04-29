using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    // クラスのインスタンス（インスペクターで設定）
    [SerializeField] FieldCard _fieldCard;
    [SerializeField] Dora _dora;

    // シングルトンクラスのインスタンス
    CardPrefabs _cardPrefabs;

    // 元となるリスト
    List<int> _cards = new List<int>(54);
    // 使われる山札となるリスト
    List<int> deckCards = new List<int>();

    // 山札の残り枚数
    public int ListCount { get; private set; }

    // 山札カードのゲームオブジェクト
    GameObject _deckCardObject;

    void Start()
    {
        // 元のリストに数字（カード）を生成
        for (var i = 0; i < 54; i++)
        {
            _cards.Add(i); 
        }

        // シングルトンのインスタンス取得
        _cardPrefabs = CardPrefabs.Instance;
    }

    // オブジェクトをプールに返却
    public void ReturnObject()
    {
        // 位置と親オブジェクトをリセット
        _deckCardObject.transform.localPosition = Vector3.zero;
        _deckCardObject.transform.SetParent(null);

        // プールに返却
        _cardPrefabs.ReturnCardObject(_deckCardObject);
    }

    // デッキを初期化
    public void InitializeDeck()
    {
        // 使用するリストを空にし、新しく元のリストから代入しシャッフル
        deckCards.Clear();
        deckCards = new List<int>(_cards);
        ShuffleDeck();

        // 山札のカード受け取り設定
        _deckCardObject = _cardPrefabs.GetCardObject();
    }

    // デッキをシャッフル
    void ShuffleDeck()
    {
        // ランダムなインデックスでカードをシャッフル
        for (int i = 0; i < deckCards.Count; i++)
        {
            // ランダムに選んだカードと交換するのを繰り返す
            int randomIndex = Random.Range(0, deckCards.Count);
            int temp = deckCards[i];
            deckCards[i] = deckCards[randomIndex];
            deckCards[randomIndex] = temp;
        }
    }

    // カードを引く処理（タプル）
    public (GameObject, int) DrawCard()
    {
        // 数字を代入し、リストから削除
        int card = deckCards[0];
        deckCards.RemoveAt(0);

        // 山札がなくなったら山札を補充
        if (deckCards.Count == 0)
        {
            ReplenishDeck(_fieldCard.boardCards);
        }

        // 現在の山札のオブジェクトを渡す
        GameObject obj = _deckCardObject;

        // 新しくオブジェクトをプールから取得し、各種設定
        _deckCardObject = _cardPrefabs.GetCardObject();
        _cardPrefabs.SetBackSprite(_deckCardObject);
        _deckCardObject.transform.SetParent(transform);
        _deckCardObject.transform.localPosition = new Vector3(-1.0f, 0.25f, 0);

        // 数字とオブジェクトを返す
        return (obj, card);
    }

    // デッキにカードを補充してシャッフル
    void ReplenishDeck(List<int> cards)
    {
        // 場に出されたカードを新たな山札に補充し、シャッフル。レートを2倍に
        deckCards.AddRange(cards.GetRange(0, cards.Count - 1));
        ShuffleDeck();
        Game.Instance._bonus *= 2;

        // 場の札のリストを空にし、現在の場のカードを代入
        int listElement = cards[cards.Count - 1];
        _fieldCard.boardCards.Clear();
        _fieldCard.boardCards.Add(listElement);

        // 裏ドラがジョーカーかによってジョーカーの枚数をリセット
        if (_dora.number == 52 || _dora.number == 53)
        {
            BasePlayer.OllJoker = 1;
        }
        else
        {
            BasePlayer.OllJoker = 2;
        }
    }

    // 山札の残りの枚数を取得
    void Update()
    {
        ListCount = deckCards.Count;
    }
}
