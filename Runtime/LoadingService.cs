using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace ContextLoaderService.Runtime
{
    public class LoadingService : IInitializable, IDisposable
    {
        private readonly ReactiveProperty<LoadingData> _state;
        private readonly Subject<float> _progress = new Subject<float>();
        private readonly LoadingView _loadingView;
        private readonly CancellationTokenSource _cancellationToken;

        public IObservable<LoadingData> State => _state;
        public IObservable<float> Progress => _progress;
        public LoadingView LoadingView => _loadingView;

        
        
        public LoadingService(LoadingView loadingView)
        {
            _cancellationToken = new CancellationTokenSource();
            _state = new ReactiveProperty<LoadingData>(new LoadingData()
            {
                LoadingState = Runtime.State.Idle,
                ShowCancelDelay = -1
            });
            _loadingView = loadingView;
        }

        public void Initialize()
        {
            _loadingView.Initialize(this);
            _loadingView.CancelSubject.Subscribe(unit =>
            {
                CancelCurrentLoadings();
            });
        }

        private IDisposable _disposable;
        
        public async UniTask BeginLoading(double delay, double showCancelDelay, string loadingType,
            params ILoadUnit[] units)
        {
            _disposable?.Dispose();
            try
            {
                _state.Value = SetState(Runtime.State.Loading, showCancelDelay, loadingType);
                //_loadingView.SetShowCancelTimer(showCancelDelay);
                _progress.OnNext(0f);
                for (int i = 0; i < units.Length; i++)
                {
                    var loadUnit = units[i];
                    Debug.Log($"<color=orange>Begin Loading</color>: {loadUnit.GetType().Name}");
                    var index = i;
                    loadUnit.Progress.Subscribe(p => _progress.OnNext(p * ((1f + index) / units.Length)));
                    await loadUnit.Load(_cancellationToken.Token);
                    Debug.Log($"<color=cyan>End Loading</color>: {loadUnit.GetType().Name}");
                }

                _progress.OnNext(1f);

                if (delay > 0)
                {
                    _disposable = Observable.Timer(TimeSpan.FromSeconds(delay)).Subscribe(l => _state.Value = SetState(Runtime.State.Idle));
                }
                else
                {
                    _state.Value = SetState(Runtime.State.Idle);
                }

            }
            catch (Exception e)
            {
                Debug.LogError(e);
                _state.Value = SetState(Runtime.State.Idle);
                throw;
            }
        }
        
        public UniTask BeginLoading(params ILoadUnit[] units)
        {
            return BeginLoading(-1, -1, String.Empty, units);
        }
        
        public UniTask BeginLoading(string loadingType, params ILoadUnit[] units)
        {
            return BeginLoading(-1, -1, loadingType, units);
        }
        
        public UniTask BeginLoading(double showCancelDelay, params ILoadUnit[] units)
        {
            return BeginLoading(-1, showCancelDelay, String.Empty, units);
        }

        public UniTask BeginLoadingWithDelay(double delay, params ILoadUnit[] units)
        {
            return BeginLoading(delay, -1, String.Empty, units);
        }
        
        public async UniTask BeginLoadingParallel(params ILoadUnit[] units)
        {
            try
            {
                _state.Value = SetState(Runtime.State.Idle);
                Debug.Log("<color=orange>Begin Loading</color>: Parallel.");
                UniTask t = UniTask.WhenAll(units.Select(e => e.Load(_cancellationToken.Token)));
                await t;
                Debug.Log("<color=orange>End Loading</color>: Parallel.");
                _state.Value = SetState(Runtime.State.Idle);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                _state.Value = SetState(Runtime.State.Idle);
                throw;
            }
        }

        public void CancelCurrentLoadings()
        {
            _cancellationToken.Cancel();
        }

        private LoadingData SetState(State state, double showCancelDelay = -1, string loadingType = "")
        {
            return new LoadingData
            {
                LoadingState = state,
                ShowCancelDelay = showCancelDelay,
                LoadingType = loadingType
            };
        }
        
        public void Dispose()
        {
            CancelCurrentLoadings();
            _loadingView.Dispose();
            _state?.Dispose();
        }
    }
}