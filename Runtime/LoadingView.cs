using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ContextLoaderService.Runtime
{
    public class LoadingView : MonoBehaviour, IInitializable, IDisposable
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private List<GameObject> loadingDimmers;
        [SerializeField] private GameObject defaultLoadingDimmer;
        [SerializeField] private Button cancelButton;
        [SerializeField] private float loadingStateThrottle = 0.5f;

        private LoadingService _loadingService;
        private IDisposable _stateChangeDisposable;

        private readonly Subject<Unit> _cancelSubject = new Subject<Unit>();
        private IDisposable _cancelTimerDisposable;
        
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public Canvas Canvas => canvas;
        public Subject<Unit> CancelSubject => _cancelSubject;
        public string[] LoadingViewTypes => loadingDimmers.Select(obj => obj.name).ToArray();

        
        [Inject]
        private void Inject(LoadingService loadingService)
        {
            _loadingService = loadingService;
            _loadingService.RegisterLoadingView(this);
        }
        
        public void Initialize()
        {
            _loadingService.State.Debounce(TimeSpan.FromSeconds(loadingStateThrottle)).Subscribe(OnLoadingServiceStateChanged).AddTo(_disposable);
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
            if (string.IsNullOrEmpty(newState.LoadingType))
            {
                defaultLoadingDimmer.SetActive(newState.LoadingState == State.Loading);
                loadingDimmers.ForEach(obj => obj.SetActive(false));
            }
            else
            {
                loadingDimmers.ForEach(obj =>
                    obj.SetActive(obj.name.Equals(newState.LoadingType) && newState.LoadingState == State.Loading));
            }
            
            if (newState.LoadingState == State.Loading)
            {
                ShowCancelButton(false);
                if (newState.ShowCancelDelay < 0)
                {
                    return;
                }
                _cancelTimerDisposable = Observable.Timer(TimeSpan.FromSeconds(newState.ShowCancelDelay), UnityTimeProvider.TimeUpdateIgnoreTimeScale).Subscribe(l => ShowCancelButton());
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