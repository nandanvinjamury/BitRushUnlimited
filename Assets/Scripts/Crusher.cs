using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace bitrush {
    public class Crusher : MonoBehaviour {
        [SerializeField] private Transform _crusherTransform;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private LayerMask _groundLayer;

        [SerializeField] private float _crushSpeed = 50f;
        [SerializeField] private int _crushDelay = 250; //milliseconds
        [SerializeField] private float _riseSpeed = 10f;
        [SerializeField] private int _riseDelay = 250; //milliseconds

        private Vector2 _crusherPosition;
        private Vector2 _finalPosition;

        private void Awake() {
            _crusherPosition = _crusherTransform.position;
            RaycastHit2D hit = Physics2D.Raycast(_crusherPosition, Vector2.down, Mathf.Infinity, _groundLayer);
            if (!hit) {
                Debug.LogError("Crusher requires a ground layer to fall on!");
                Destroy(this);
            }

            _finalPosition = hit.point;
        }

        private async UniTaskVoid Crush() {
            await UniTask.Delay(_crushDelay);
            while (_crusherTransform != null && Vector3.Distance(_crusherTransform.position, _finalPosition) > 0.15f) {
                _crusherTransform.position = Vector3.MoveTowards(_crusherTransform.position, _finalPosition, _crushSpeed * Time.deltaTime);
                _lineRenderer.SetPosition(1, new Vector2(_crusherPosition.x, _crusherTransform.position.y+2));
                await UniTask.Yield();
            }
            Rise().Forget();
        }

        private async UniTaskVoid Rise() {
            await UniTask.Delay(_riseDelay);
            while (_crusherTransform != null && Vector3.Distance(_crusherTransform.position, _crusherPosition) > 0.15f) {
                _crusherTransform.position = Vector3.MoveTowards(_crusherTransform.position, _crusherPosition, _riseSpeed * Time.deltaTime);
                _lineRenderer.SetPosition(1, new Vector2(_crusherPosition.x, _crusherTransform.position.y+2));
                await UniTask.Yield();
            }
        }

        private void OnTriggerEnter2D(Collider2D col) {
            if(col.gameObject.layer == LayerMask.NameToLayer("Player")){
                Crush().Forget();
            }
        }
    }
}