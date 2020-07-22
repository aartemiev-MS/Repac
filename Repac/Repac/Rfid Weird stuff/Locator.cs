using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Repac.Rfid_Weird_stuff
{
    public class Locator
    {
        private static readonly object instanceLock = new object();
        private static Locator defaultInstance;

        private List<object> instances;
        private IDictionary<Type, Delegate> factories;

        public Locator()
        {
            this.instances = new List<object>();
            this.factories = new Dictionary<Type, Delegate>();
        }

        /// <summary>
        /// Gets the singleton default instance of the Locator class
        /// </summary>
        public static Locator Default
        {
            get
            {
                if (defaultInstance == null)
                {
                    lock (instanceLock)
                    {
                        defaultInstance = new Locator();
                    }
                }

                return defaultInstance;
            }
        }

        public bool IsRegistered<TClass>()
        {
            return this.factories.ContainsKey(typeof(TClass));
        }

        public void Register<TClass>()
        {
            this.Register<TClass>(() => this.Construct<TClass>());
        }

        public void Register<TInterface, TClass>() where TClass : TInterface
        {
            this.Register<TClass>();
            this.Register<TInterface>(() => { return this.Locate<TClass>(); });
        }

        public void Register(object instance)
        {
            this.instances.Add(instance ?? throw new ArgumentNullException("instance"));
        }

        public void Register<TViewModel>(Func<TViewModel> factory, bool createImmediately = false)
        {
            this.factories.Add(typeof(TViewModel), factory ?? throw new ArgumentNullException("factory"));
            if (createImmediately)
            {
                this.instances.Add(factory());
            }
        }

        public TViewModel Locate<TViewModel>()
        {
            TViewModel instance = this.instances.OfType<TViewModel>().FirstOrDefault();

            if (instance == null)
            {
                Type requiredType = typeof(TViewModel);
                if (this.factories.ContainsKey(requiredType))
                {
                    instance = (TViewModel)this.factories[requiredType].DynamicInvoke(null);

                    if (instance != null)
                    {
                        this.instances.Add(instance);
                    }
                }
            }

            return instance;
        }

        public object GetInstance(Type requiredType)
        {
            object instance = this.instances.Where(x => x.GetType() == requiredType).FirstOrDefault();

            if (instance == null)
            {
                if (this.factories.ContainsKey(requiredType))
                {
                    instance = this.factories[requiredType].DynamicInvoke(null);

                    if (instance != null)
                    {
                        this.instances.Add(instance);
                    }
                }
            }

            return instance;
        }

        public TClass Construct<TClass>() //where TClass : class
        {
            var constuctorInfo = typeof(TClass).GetConstructors().Single();

            var parameters = constuctorInfo.GetParameters().Select(x => this.GetInstance(x.ParameterType)).ToArray();
            return (TClass)constuctorInfo.Invoke(parameters);
        }
    }
}
