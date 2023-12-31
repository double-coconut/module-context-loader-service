using ContextLoaderService.Runtime.BaseUnits;
using Cysharp.Threading.Tasks;

namespace ContextLoaderService.Runtime
{
    public static class Extensions
    {
        public static UniLoadingUnit ToLoadingUnit(this UniTask task)
        {
            return new UniLoadingUnit(task);
        }

        public static UniLoadingUnit<T> ToLoadingUnit<T>(this UniTask<T> task)
        {
            return new UniLoadingUnit<T>(task);
        }
    }
}