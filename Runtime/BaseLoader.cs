using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Zenject;

namespace ContextLoaderService.Runtime
{
    public abstract class BaseLoader<T> : BaseLoader where T : ILoadData
    {
        protected readonly T Data;

        protected BaseLoader(LoadingService loadingService, ILoadData data) : base(loadingService)
        {
            if (!(data is T loadData))
            {
                throw new InvalidDataException($"Load data is Null or has different type than {typeof(T)}");
            }

            Data = loadData;
        }
    }

    public abstract class BaseLoader : IDisposable, IInitializable
    {
        protected readonly LoadingService LoadingService;
        protected State LoadingStatus { get; private set; }


        protected BaseLoader(LoadingService loadingService)
        {
            LoadingService = loadingService;
            LoadingStatus = State.Idle;
        }

        public virtual void Initialize()
        {
            LoadInternal().Forget();
        }

        public async UniTask AwaitCompletion()
        {
            while (LoadingStatus != State.Loaded)
            {
                await UniTask.DelayFrame(1);
            }
        }
        
        
        private async UniTask LoadInternal()
        {
            LoadingStatus = State.Loading;
            await InitWithDelay();
            LoadingStatus = State.Loaded;
        }
        
        protected abstract UniTask Load();

        //This should be called because of the scene loading is one frame faster then Loading service's awaiter.
        protected async UniTask InitWithDelay()
        {
            await UniTask.DelayFrame(1);
            await Load();
        }

        public virtual void Dispose()
        {
        }
    }
}