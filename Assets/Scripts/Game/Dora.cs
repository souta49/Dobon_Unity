using System.Collections;
using UnityEngine;

public class Dora : MonoBehaviour
{
    // 各クラスのインスタンス（インスペクターで設定）
    [SerializeField] FieldCard _field;

    // ドラのオブジェクト
    GameObject _dora;

    // ドラの数字（参照のみ）
    public int number { get; private set; }

    // オブジェクトをプールに返却
    public void ReturnObject()
    {
        // 位置と親オブジェクトをリセット
        _dora.transform.localPosition = Vector3.zero;
        _dora.transform.SetParent(null);

        // プールに返却
        CardPrefabs.Instance.ReturnCardObject(_dora);
    }

    // 初期化
    public void ClearDora()
    {
        // 変数を空に
        _dora = null;
    }

    // ドラのカードを受け取る処理
    public IEnumerator ReceiveDora(GameObject obj, int card)
    {
        // オブジェクトと数字を代入し、裏向きで描画
        _dora = obj;
        number = card;
        _dora.transform.SetParent(transform);

        // カードを1秒で移動させるコルーチンを開始
        Vector3 targetPosition = new Vector3(-5.0f, 0.25f, 0);
        yield return StartCoroutine(CoroutineManager.Instance.MoveCardToPosition(_dora, targetPosition, 1f));
    }

    // ドラをオープンする処理
    public void OpenDora()
    {
        // 辞書からオブジェクトのスプライトレンダラー取得
        var spriteRenderer = CardPrefabs.Instance.spriteRendererCache[_dora];

        // 現在の裏向きのスプライトを取得
        Sprite backSprite = spriteRenderer.sprite;

        // 表向きに変更
        CardPrefabs.Instance.SetCardSprite(_dora, number);

        // 表のスプライトを取得
        Sprite frontSprite = spriteRenderer.sprite;

        // スプライトレンダラーと表、裏のスプライトを渡してコルーチン開始
        StartCoroutine(CoroutineManager.Instance.FlipCard(spriteRenderer, frontSprite, backSprite));
    }

    // ドラチェック
    public void CheakDora()
    {
        // 場の数と同じかジョーカーならレート2倍
        if (_field._fieldNumber == number % 13 + 1 || number == 52 || number == 53)
        {
            Game.Instance._bonus *= 2;
        }
    }
}
