using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Services.Loading.BaseUnits
{
    public class SceneLoadUnit<T> : ILoadUnit //where T : struct
    {
        private readonly T _scene;
        private readonly LoadSceneMode _mode;

        private readonly Subject<float> _progress = new Subject<float>();
        public IObservable<float> Progress => _progress;


        public SceneLoadUnit(T scene, LoadSceneMode mode)
        {
            _scene = scene;
            _mode = mode;
        }

        public void Dispose()
        {
        }

        void IProgress<float>.Report(float value)
        {
            _progress.OnNext(value);
        }

        public async UniTask Load(CancellationToken cancellationToken = default)
        {
            switch (_scene)
            {
                case string stringScene:
                    await SceneManager.LoadSceneAsync(stringScene, _mode)
                        .ToUniTask(progress: this, cancellationToken: cancellationToken);
                    break;
                case int intScene:
                    await SceneManager.LoadSceneAsync(intScene, _mode)
                        .ToUniTask(progress: this, cancellationToken: cancellationToken);
                    break;
                default:
                    Debug.LogError(
                        $"Your scene is not match with the scene format, it should be int or string! : {typeof(T).Name}");
                    break;
            }
        }
    }
}