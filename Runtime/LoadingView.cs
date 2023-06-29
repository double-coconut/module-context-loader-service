using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace ContextLoaderService.Runtime
{
    public class LoadingView : MonoBehaviour, IDisposable
    {
        [SerializeField] private GameObject loadingDimmer;
        [SerializeField] private Button cancelButton;

        private LoadingService _loadingService;
        private IDisposable _stateChangeDisposable;

        private readonly Subject<Unit> _cancelSubject = new Subject<Unit>();
        private IDisposable _cancelTimerDisposable;
        
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public Subject<Unit> CancelSubject => _cancelSubject;

        public void Initialize(LoadingService loadingService)
        {
            _loadingService = loadingService;
            _loadingService.State.Subscribe(OnLoadingServiceStateChanged).AddTo(_disposable);
            _disposable.Add(_cancelSubject);
            cancelButton.onClick.AsObservable().Subscribe(unit => _cancelSubject?.OnNext(Unit.Default)).AddTo(_disposable);;
            DontDestroyOnLoad(gameObject);
        }

        public void SetShowCancelTimer(double delay)
        {
            _cancelTimerDisposable?.Dispose();
            
            ShowCancelButton();
        }
        
        private void OnLoadingServiceStateChanged(LoadingData newState)
        {
            loadingDimmer.SetActive(newState.LoadingState == State.Loading);
            
            if (newState.LoadingState == State.Loading)
            {
                ShowCancelButton(false);
                if (newState.ShowCancelDelay < 0)
                {
                    return;
                }
                _cancelTimerDisposable = Observable.Timer(TimeSpan.FromSeconds(newState.ShowCancelDelay), Scheduler.MainThreadIgnoreTimeScale).Subscribe(l => ShowCancelButton());
            }
            else
            {
                ShowCancelButton(false);
                _cancelTimerDisposable?.Dispose();
            }
        }

        private void ShowCancelButton(bool show = true)
        {
            cancelButton.gameObject.SetActive(show);
        }
        
        public void Dispose()
        {
            _disposable.Dispose();
            if (gameObject!=null)
            {
                Destroy(gameObject);
            }
        }
    }
}