using System;
using UnityEngine;
using Zenject;

namespace ContextLoaderService.Runtime
{
    [RequireComponent(typeof(Context))]
    public class LoadingContextRunner : MonoBehaviour
    {
        [SerializeField] private RunnableContext context;
        [SerializeField] private ContainerSources containerSource = ContainerSources.SearchHierarchy;

        private bool _hasInjected;

        public ContainerSources ContainerSource
        {
            get => containerSource;
            set => containerSource = value;
        }

        public DiContainer Container => context.Container;

        // Make sure they don't cause injection to happen twice
        [Inject]
        private void Construct()
        {
            if (!_hasInjected)
            {
                throw new Exception(
                    "ZenAutoInjecter was injected!  Do not use ZenAutoInjecter for objects that are instantiated through zenject or which exist in the initial scene hierarchy");
            }
        }

        public void Run(ILoadData data = default)
        {
            _hasInjected = true;
            context.Container.Bind<ILoadData>().FromInstance(data).AsSingle().NonLazy();
            LookupContainer().InjectGameObject(gameObject);
            context.Run();
        }

        private DiContainer LookupContainer()
        {
            switch (containerSource)
            {
                case ContainerSources.ProjectContext:
                    return ProjectContext.Instance.Container;
                case ContainerSources.SceneContext:
                    return GetContainerForCurrentScene();
                default:
                {
                    var parentContext = transform.GetComponentInParent<Context>();
                    return parentContext != null ? parentContext.Container : GetContainerForCurrentScene();
                }
            }
        }

        private DiContainer GetContainerForCurrentScene()
        {
            return ProjectContext.Instance.Container.Resolve<SceneContextRegistry>()
                .GetContainerForScene(gameObject.scene);
        }

        [Serializable]
        public enum ContainerSources
        {
            SceneContext,
            ProjectContext,
            SearchHierarchy
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            context = GetComponent<RunnableContext>();
        }
#endif
    }
}