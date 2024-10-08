using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Zenject;
using Logger = DCLogger.Runtime.Logger;

namespace ContextLoaderService.Runtime
{
    public class LoadingService : IInitializable, IDisposable
    {
        private readonly ReactiveProperty<LoadingData> _state;
        private readonly Subject<float> _progress = new();
        private readonly CancellationTokenSource _cancellationToken;
        private LoadingView _loadingView;
        private IDisposable _disposable;


        public LoadingView LoadingView => _loadingView;

        public Observable<LoadingData> State => _state;
        public Observable<float> Progress => _progress;

        public LoadingService()
        {
            _cancellationToken = new CancellationTokenSource();
            _state = new ReactiveProperty<LoadingData>(new LoadingData()
            {
                LoadingState = Runtime.State.Idle,
                ShowCancelDelay = -1
            });
        }

        public void Initialize()
        {
        }

        
        public void RegisterLoadingView(LoadingView loadingView)
        {
            _loadingView = loadingView;
            _loadingView?.CancelSubject.Subscribe(unit =>
            {
                CancelCurrentLoadings();
            });
#if DC_LOGGING
                    Logger.Log($"Loading view registered. Name: {loadingView?.name}",
                        ContextLoaderLogChannels.Default);
#else
            Debug.Log($"Loading view registered. Name: {loadingView?.name}");
#endif
            
        }
        
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
#if DC_LOGGING
                    Logger.Log($"<color=orange>Begin Loading</color>: {loadUnit.GetType().Name}",
                        ContextLoaderLogChannels.Default);
#else
                    Debug.Log($"<color=orange>Begin Loading</color>: {loadUnit.GetType().Name}");
#endif
                    var index = i;
                    loadUnit.Progress.ToObservable()
                        .Subscribe(p => _progress.OnNext(p * ((1f + index) / units.Length)));
                    await loadUnit.Load(_cancellationToken.Token);
#if DC_LOGGING
                    Logger.Log($"<color=cyan>End Loading</color>: {loadUnit.GetType().Name}",
                        ContextLoaderLogChannels.Default);
#else
                    Debug.Log($"<color=cyan>End Loading</color>: {loadUnit.GetType().Name}");
#endif
                }

                _progress.OnNext(1f);

                if (delay > 0)
                {
                    _disposable = Observable.Timer(TimeSpan.FromSeconds(delay))
                        .Subscribe(l => _state.Value = SetState(Runtime.State.Idle));
                }
                else
                {
                    _state.Value = SetState(Runtime.State.Idle);
                }
            }
            catch (Exception e)
            {
#if DC_LOGGING
                Logger.LogError(e.ToString(), ContextLoaderLogChannels.Error);
#else
                Debug.LogError(e);
#endif
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
#if DC_LOGGING
                Logger.Log("<color=orange>Begin Loading</color>: Parallel.", ContextLoaderLogChannels.Default);
#else
                Debug.Log("<color=orange>Begin Loading</color>: Parallel.");
#endif
                UniTask t = UniTask.WhenAll(units.Select(e => e.Load(_cancellationToken.Token)));
                await t;
#if DC_LOGGING
                Logger.Log("<color=orange>End Loading</color>: Parallel.", ContextLoaderLogChannels.Default);
#else
                Debug.Log("<color=orange>End Loading</color>: Parallel.");
#endif
                _state.Value = SetState(Runtime.State.Idle);
            }
            catch (Exception e)
            {
#if DC_LOGGING
                Logger.LogError(e.ToString(), ContextLoaderLogChannels.Error);
#else
                Debug.LogError(e);
#endif
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