namespace MisterGames.Common.Dependencies {

    public static class DependencyResolverExtensions {

        public static IDependencyResolver Resolve<T>(this IDependencyResolver resolver, out T dependency) where T : class {
            dependency = resolver.Resolve<T>();
            return resolver;
        }
    }

}
