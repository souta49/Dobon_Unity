using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class CardPrefabs : SingletonMonoBehaviour<CardPrefabs>// シングルトンクラス
{
    // カードのプレファブとスプライトのインスタンス（インスペクターで設定）
    [SerializeField] Sprite _backSprite;
    [SerializeField] Sprite _doraSprite;
    [SerializeField] Sprite[] _sprites;
    [SerializeField] GameObject _cardPrefab;

    // プロパティ
    public Sprite[] Sprites { get { return _sprites; } }

    // オブジェクトプール
    ObjectPool<GameObject> _pool;

    // カードオブジェクトのスプライトレンダラ用辞書（参照のみ）
    // 毎回コンポーネントを取得するのではなく先にキャッシュしてパフォーマンスを向上させる
    public Dictionary<GameObject, SpriteRenderer> spriteRendererCache { get; private set; } = new Dictionary<GameObject, SpriteRenderer>();

    void Start()
    {
        // オブジェクトプールを生成
        InitializePool();
    }

    // オブジェクトプール生成処理
    void InitializePool()
    {
        // Unity内機能のプール生成処理
        _pool = new ObjectPool<GameObject>(
            createFunc: CreateCardInstance,         // プールにオブジェクトを生成する処理
            actionOnGet: OnGet,                     // プールからオブジェクトを取得する処理
            actionOnRelease: OnRelease,             // プールにオブジェクトを返却する処理
            actionOnDestroy: DestroyCardInstance,   // プールからカオブジェクトを削除する処理
            defaultCapacity: 9,                     // プール生成時に最初に生成するオブジェクトの数
            maxSize: 22                             // プールの最大値
            );
    }

    // プールにカードを生成する処理
    GameObject CreateCardInstance()
    {
        // インスタンスを取得し、スプライトレンダラをキャッシュ
        var newObj = Instantiate(_cardPrefab);
        CacheSpriteRenderer(newObj);
        return newObj;
    }

    // プールからカードを取得する処理
    void OnGet(GameObject obj)
    {
        // オブジェクトをアクティブにする
        obj.SetActive(true);
    }

    // プールにカードを返却する処理
    void OnRelease(GameObject obj)
    {
        // オブジェクトを非アクティブにする
        obj.SetActive(false);
    }

    // プールからカードを削除する処理（使用しないが、メソッドは必要）
    void DestroyCardInstance(GameObject obj)
    {
        Destroy(obj);
    }

    // カードオブジェクトを取得するメソッド
    public GameObject GetCardObject()
    {
        return _pool.Get();
    }

    // プールにオブジェクトを返却するメソッド
    public void ReturnCardObject(GameObject obj)
    {
        _pool.Release(obj);
    }

    // スプライトをキャッシュする処理　
    void CacheSpriteRenderer(GameObject cardObj)
    {
        // オブジェクトにスプライトレンダラーコンポーネントがあったら
        if (cardObj.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            // 辞書にオブジェクトとペアでキャッシュ
            spriteRendererCache[cardObj] = spriteRenderer;
        }
    }

    // オブジェクトの明るさをリセット
    public void ResetWhiteColor(GameObject obj)
    {
        // 辞書から取得し、明るさを白に
        var spriteRenderer = spriteRendererCache[obj];
        spriteRenderer.color = Color.white;
    }

    public void ResetGrayColor(GameObject obj)
    {
        // 辞書から取得し、明るさをグレーに
        var spriteRenderer = spriteRendererCache[obj];
        spriteRenderer.color = Color.gray;
    }

    // 表のカードのスプライトを設定
    public void SetCardSprite(GameObject cardObj, int cardIndex)
    {
        // 辞書から取得し、表向きに描画
        var spriteRenderer = spriteRendererCache[cardObj];
        spriteRenderer.sprite = Sprites[cardIndex];
    }

    // 裏のカードのスプライトを設定
    public void SetBackSprite(GameObject cardObj)
    {
        // 辞書から取得し、裏向きに描画
        var spriteRenderer = spriteRendererCache[cardObj];
        spriteRenderer.sprite = _backSprite;
    }

    // ドラのカードのスプライトを設定
    public void SetDoraSprite(GameObject cardObj)
    {
        // 辞書から取得し、裏向きに描画
        var spriteRenderer = spriteRendererCache[cardObj];
        spriteRenderer.sprite = _doraSprite;
    }
}
