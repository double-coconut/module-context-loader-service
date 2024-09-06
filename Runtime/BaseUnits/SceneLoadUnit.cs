using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = DCLogger.Runtime.Logger;

namespace ContextLoaderService.Runtime.BaseUnits
{
    public class SceneLoadUnit<T> : ILoadUnit
    {
        private readonly T _scene;
        private readonly LoadSceneMode _mode;

        private readonly Subject<float> _progress = new Subject<float>();
        public IObservable<float> Progress => _progress.AsSystemObservable();


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
            try
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
#if DC_LOGGING
                        Logger.LogError(
                            $"Your scene is not match with the scene format, it should be int or string! : {typeof(T).Name}",
                            ContextLoaderLogChannels.Error);
#else
                  Debug.LogError(
                            $"Your scene is not match with the scene format, it should be int or string! : {typeof(T).Name}");
#endif
      
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