using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Services.Loading
{
    public interface ILoadingFlow : IDisposable
    {
        public interface IData
        {
        }

        UniTask Initialize(IData data = null);
    }
}