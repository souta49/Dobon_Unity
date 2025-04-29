using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldCard : MonoBehaviour
{ 
    //場の数字（参照のみ）
    public int _fieldNumber { get; private set; }

    // 場のスート（数字と文字）
    public int _fieldSuit { get; set; }
    public string _suitText;

    // 場の札のリスト
    public List<int> boardCards = new List<int>();

    // 場のカードのゲームオブジェクト
    GameObject _currentCardObject;

    // オブジェクトをプールに返却
    public void ReturnObject()
    {
        // 位置と親オブジェクトをリセット
        _currentCardObject.transform.localPosition = Vector3.zero;
        _currentCardObject.transform.SetParent(null);

        // プールに返却
        CardPrefabs.Instance.ReturnCardObject(_currentCardObject);
    }

    // 初期化
    public void ClearBoard()
    {
        // リストと変数を空にする
        boardCards.Clear();
        _currentCardObject = null;
    }

    // 場にカードを出す処理
    public IEnumerator AddCardToBoard(GameObject obj, int card)
    {
        // 現在のオブジェクトをプールに返却
        if (_currentCardObject != null)
        {
            ReturnObject();
        }

        // 受け取ったカードを各種設定
        _currentCardObject = obj;
        _currentCardObject.transform.SetParent(transform);
        _currentCardObject.name = card.ToString();

        // 場のリストに追加
        boardCards.Add(card);

        // 数字とスートを更新
        _fieldNumber = card % 13 + 1;
        _fieldSuit = card / 13;

        // カードを1秒で移動させるコルーチンを開始
        Vector3 targetPosition = new Vector3(-3.0f, 0.25f, 0);
        yield return StartCoroutine(CoroutineManager.Instance.MoveCardToPosition(_currentCardObject, targetPosition, 1f));
    }

    // スートを常に取得
    void Update()
    {
        // テキストにスートを表示
        suitText(_fieldSuit);
    }

    // テキスト表示用処理
    string suitText(int suit)
    {
        // スートの数字でマークを表示
        switch(suit)
        {
            case 0: return _suitText = "♠";
            case 1: return _suitText = "♥";
            case 2: return _suitText = "♦";
            case 3: return _suitText = "♣";
            default: return _suitText = "?"; 
        }
    }
}
