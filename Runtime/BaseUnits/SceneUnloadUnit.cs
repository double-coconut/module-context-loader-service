using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ContextLoaderService.Runtime.BaseUnits
{
    public class SceneUnloadUnit<T> : ILoadUnit
    {
        private readonly T _scene;
        private readonly Subject<float> _progress = new Subject<float>();
        public IObservable<float> Progress => _progress.AsSystemObservable();

        public SceneUnloadUnit(T scene)
        {
            _scene = scene;
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
            try
            {
                switch (_scene)
                {
                    case string stringScene:
                        await SceneManager.UnloadSceneAsync(stringScene)
                            .ToUniTask(progress: this, cancellationToken: cancellationToken);
                        break;
                    case int intScene:
                        await SceneManager.UnloadSceneAsync(intScene)
                            .ToUniTask(progress: this, cancellationToken: cancellationToken);
                        break;
                    default:
                        Debug.LogError(
                            $"SceneUnLoadUnit - Your scene is not match with the scene format, it should be int or string! : {typeof(T).Name}");
                        break;
                }
            }
            catch (Exception e)
            {
                throw;
            }

            await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
        }
    }
}