using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Services.Loading.BaseUnits
{
    public class UniLoadingUnit : ILoadUnit
    {
        private readonly UniTask _task;
        private readonly CompositeDisposable _disposable;

        public UniLoadingUnit(UniTask task)
        {
            _disposable = new CompositeDisposable();
            _task = task;
        }

        public IObservable<float> Progress => new Subject<float>().AddTo(_disposable);


        public UniTask Load(CancellationToken cancellationToken = default)
        {
            return _task.AttachExternalCancellation(cancellationToken);
        }


        void IProgress<float>.Report(float value)
        {
        }

        void IDisposable.Dispose()
        {
            _disposable?.Dispose();
        }
    }

    public class UniLoadingUnit<T> : ILoadUnit<T>
    {
        private readonly UniTask<T> _task;
        private readonly CompositeDisposable _disposable;


        public T Result { get; private set; }


        public UniLoadingUnit(UniTask<T> task)
        {
            _disposable = new CompositeDisposable();
            _task = task;
        }

        public IObservable<float> Progress => new Subject<float>().AddTo(_disposable);


        public async UniTask Load(CancellationToken cancellationToken = default)
        {
            T result = await _task.AttachExternalCancellation(cancellationToken);
            Result = result;
        }


        void IProgress<float>.Report(float value)
        {
        }

        void IDisposable.Dispose()
        {
            _disposable?.Dispose();
        }
    }
}