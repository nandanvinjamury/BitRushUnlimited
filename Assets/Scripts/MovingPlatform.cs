using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace bitrush {
    public class MovingPlatform : MonoBehaviour {

        [SerializeField] private float _moveSpeed = 20f;
        [SerializeField] private int _moveDelay = 250; //milliseconds
        [SerializeField] private bool _cycle = true;
        [SerializeField] private Vector3[] _waypoints;

        private CancellationTokenSource _cts;

        private int _nonCycleIterator = 1;
        private void Start() {
            if(_waypoints.Length < 2) {
                Debug.LogError("Moving Platform requires at least 2 waypoints");
                Destroy(this);
            }
            _cts = new CancellationTokenSource();
            Move(_cts.Token);
        }

        private async void Move(CancellationToken token) {
            await Task.Delay(_moveDelay);
            int index = 0;
            while (!token.IsCancellationRequested) {
                transform.position = Vector3.MoveTowards(transform.position, _waypoints[index], _moveSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, _waypoints[index]) < 0.1f) {
                    if (_cycle) {
                        index++;
                        if (index == _waypoints.Length) {
                            index = 0;
                        }
                    } else {
                        index += _nonCycleIterator;
                        if (index == _waypoints.Length || index < 0) {
                            _nonCycleIterator *= -1;
                            index += _nonCycleIterator;
                        }
                    }
                    await Task.Delay(_moveDelay);
                } else {
                    await Task.Yield();
                }
            }
        }

        private void OnDestroy() {
            if (_cts != null) {
                _cts.Cancel();
                _cts.Dispose();
            }
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.green;
            for (int i = 0; i < _waypoints.Length; i++) {
                Gizmos.DrawSphere(_waypoints[i], 0.5f);
                if (i == _waypoints.Length - 1 && _cycle) {
                    Gizmos.DrawLine(_waypoints[i], _waypoints[0]);
                } else {
                    Gizmos.DrawLine(_waypoints[i], _waypoints[i + 1]);
                }
            }
        }
    }
}