using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace bitrush {
    public class FallingPlatform : MonoBehaviour {

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private BoxCollider2D _boxCollider;
        [SerializeField] private float _fallSpeed = 20f;
        [SerializeField] private int _fallDelay = 250; //milliseconds
        [SerializeField] private float _fallRespawn = 2f;

        private void OnCollisionEnter2D(Collision2D col) {
            if(col.gameObject.layer == LayerMask.NameToLayer("Player")){
                Fall().Forget();
            }
        }

        private async UniTaskVoid Fall() {
            Color color = _spriteRenderer.color;
            _spriteRenderer.color = new Color(color.r, color.g, color.b, 0.25f);
            await UniTask.Delay(_fallDelay);
            _boxCollider.enabled = false;
            float timer = 0;
            Vector3 startPos = transform.position;
            while (timer < _fallRespawn) {
                timer += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, startPos + Vector3.down*_fallSpeed, timer);
                await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            transform.position = startPos;
            _boxCollider.enabled = true;
            _spriteRenderer.color = new Color(color.r, color.g, color.b, 1);
        }
    }
}