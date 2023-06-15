using Cysharp.Threading.Tasks;

namespace ContextLoaderService.Runtime
{
    public interface Iloadable
    {
        UniTask Load(ILoadData loadData = null);
    }
}