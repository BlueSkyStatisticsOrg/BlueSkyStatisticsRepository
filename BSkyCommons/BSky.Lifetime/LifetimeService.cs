using Microsoft.Practices.Unity;

namespace BSky.Lifetime
{
    public class LifetimeService
    {
        private static readonly LifetimeService _instance = new LifetimeService();

        private LifetimeService()
        {
        }

        public static LifetimeService Instance
        {
            get
            {
                return _instance;
            }
        }

        public IUnityContainer Container
        {
            get;
            set;
        }

    }

}
