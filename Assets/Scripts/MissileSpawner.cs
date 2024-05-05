using Cysharp.Threading.Tasks;
using UnityEngine;

namespace bitrush {
    public class MissileSpawner : MonoBehaviour {
        [SerializeField] private ParticleSystem _spawnParticles;
        [SerializeField] private GameObject _missilePrefab;

        private void Awake() {
            InvokeRepeating("ShootMissile", 3f, 5f);
        }

        private async UniTaskVoid ShootMissile() {
            _spawnParticles.Play();
            await UniTask.Delay(500, cancellationToken: this.GetCancellationTokenOnDestroy());
            _spawnParticles.Stop();
            Instantiate(_missilePrefab, transform.position, Quaternion.identity);
            await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }
}