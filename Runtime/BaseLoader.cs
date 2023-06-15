using Cysharp.Threading.Tasks;
using Zenject;

namespace Services.Loading
{
    public abstract class BaseLoader : ILoadingFlow, IInitializable
    {
        protected readonly LoadingService LoadingService;

        public BaseLoader(LoadingService loadingService)
        {
            LoadingService = loadingService;
        }
        
        public virtual void Initialize()
        {
            InitWithDelay().Forget();
        }

        public abstract UniTask Initialize(ILoadingFlow.IData data); //TODO rework this data transfer.

        protected async UniTask InitWithDelay() //This should be called because of the scene loading is one frame faster then Loading service's awaiter.
        {
            await UniTask.DelayFrame(1);
            Initialize(null);
        }
        
        public virtual void Dispose()
        {
        }
    }
}