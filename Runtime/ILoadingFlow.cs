using System;
using Cysharp.Threading.Tasks;

namespace ContextLoaderService.Runtime
{
    public interface ILoadingFlow : IDisposable
    {
        public interface IData
        {
        }

        UniTask Initialize(IData data = null);
    }
}