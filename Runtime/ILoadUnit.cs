using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ContextLoaderService.Runtime
{
    public interface ILoadUnit : IProgress<float>, IDisposable
    {
        IObservable<float> Progress { get; }
        UniTask Load(CancellationToken cancellationToken = default);
    }

    public interface ILoadUnit<out T> : ILoadUnit
    {
        T Result { get; }
    }
}