using Cysharp.Threading.Tasks;

namespace Services.Loading
{
    public interface Iloadable
    {
        UniTask Load(IloadData loadData = null);
    }
}