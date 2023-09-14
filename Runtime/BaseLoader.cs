using Cysharp.Threading.Tasks;
using Zenject;

namespace ContextLoaderService.Runtime
{
    public abstract class BaseLoader : ILoadingFlow, IInitializable
    {
        protected readonly LoadingService LoadingService;
        protected State LoadingStatus { get; private set; }


        public BaseLoader(LoadingService loadingService)
        {
            LoadingService = loadingService;
            LoadingStatus = State.Idle;
        }

        public virtual void Initialize()
        {
            LoadInternal().Forget();
        }

        private async UniTask LoadInternal()
        {
            LoadingStatus = State.Loading;
            await InitWithDelay();
            LoadingStatus = State.Loaded;
        }

        public async UniTask AwaitCompletion()
        {
            while (LoadingStatus != State.Loaded)
            {
                await UniTask.DelayFrame(1);
            }
        }

        public abstract UniTask Initialize(ILoadingFlow.IData data); //TODO rework this data transfer.

        //This should be called because of the scene loading is one frame faster then Loading service's awaiter.
        protected async UniTask InitWithDelay()
        {
            await UniTask.DelayFrame(1);
            await Initialize(null);
        }

        public virtual void Dispose()
        {
        }
    }
}