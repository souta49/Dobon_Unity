using System.Collections;
using UnityEngine;

public class CoroutineManager : SingletonMonoBehaviour<CoroutineManager> // シングルトン
{
    // 再利用してパフォーマンス向上（3秒待機と2秒待機）
    WaitForSeconds wait3 = new WaitForSeconds(3.0f);
    WaitForSeconds wait2 = new WaitForSeconds(2.0f);

    // 3秒待機
    public IEnumerator delateTime3()
    {
        yield return wait3;
    }

    // 2秒待機
    public IEnumerator delateTime2()
    {
        yield return wait2;
    }

    // カードを指定時間で移動（1,オブジェクト 2,移動位置 3,移動時間）
    public IEnumerator MoveCardToPosition(GameObject cardObj, Vector3 targetPosition, float duration)
    {
        // 開始位置（現在の位置）の保存と経過時間の初期化
        Vector3 startPosition = cardObj.transform.localPosition; 
        float elapsedTime = 0;

        // 指定時間まで繰り返す
        while (elapsedTime < duration)
        {
            // 開始位置から目標位置までを補完（時間の経過に応じて滑らかに変化）
            cardObj.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            
            // フレームごとに経過時間を更新
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // 最終的な位置を設定
        cardObj.transform.localPosition = targetPosition;
    }

    // カードをひっくり返す処理（1,スプライトレンダラー 2,表のスプライト 3,裏のスプライト）
    public IEnumerator FlipCard(SpriteRenderer spriteRenderer, Sprite frontSprite, Sprite backSprite)
    {
        // ひっくり返すのに書ける時間
        float duration = 1f;
        float elapsedTime = 0f;

        // 最初のスプライトを裏面に設定
        spriteRenderer.sprite = backSprite;

        // 1秒間繰り返す
        while (elapsedTime < duration)
        {
            // 経過時間の更新と進捗率の計算
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            // 最初の半分の時間でスケールを0.1から0に縮小
            if (progress < 0.5f)
            {
                // x軸のスケールを縮小
                spriteRenderer.transform.localScale = new Vector3(Mathf.Lerp(0.1f, 0, progress * 2), 0.1f, 1);
            }
            // 残りの半分の時間でスケールを0から0.1に拡大
            else
            {
                // スプライトを変更
                spriteRenderer.sprite = frontSprite;

                // x軸のスケールを拡大
                spriteRenderer.transform.localScale = new Vector3(Mathf.Lerp(0, 0.1f, (progress - 0.5f) * 2), 0.1f, 1);
            }
            // 次のフレームまで待機
            yield return null;
        }

        // スケールを元の大きさに設定
        spriteRenderer.transform.localScale = new Vector3(0.1f, 0.1f, 1);
    }
}
