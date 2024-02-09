
internal class CustomDependencyInjector
{
    private static void Main(string[] args)
    {
        //creating object of di
        var container = new DependencyInjectContainer();
        //register dependencies into dependency container
        container.AddSingleton<X>();
        container.AddSingleton<YFunction>();
        container.AddSingleton<Z>();
        //create an object of resolver 
        var resolver = new DependencyResolver(container);
        //resolving dependencies
        var dependentservice = resolver.CreateDependencyInstance<YFunction>();
        var dependentservice1 = resolver.CreateDependencyInstance<YFunction>();
        var dependentservice2 = resolver.CreateDependencyInstance<YFunction>();
        dependentservice.SayHello();
        dependentservice1.SayHello();
        dependentservice2.SayHello();
    }
    //create a class which represents dependency
    public class Dependency
    {
        public Dependency(Type t, DependencyLifetimes l)
        {
            Type = t;
            LifeTime = l;
        }
        public Type Type { get; set; }
        public DependencyLifetimes LifeTime { get; set; }
        public bool IsImplemented { get; set; } = false;
        public System.Object Implementation { get; set; }
        public void AddImplementation(Object o)
        {
            Implementation = o;
            IsImplemented = true;
        }
    }
    //create a enum for lifetimes
    public enum DependencyLifetimes
    {
        singleton = 0,
        transient = 1,
    }
    //create a dependency resolver
    public class DependencyResolver
    {
        DependencyInjectContainer _container;
        public DependencyResolver(DependencyInjectContainer dependencyInjectContainer)
        {
            _container = dependencyInjectContainer;
        }
        public T CreateDependencyInstance<T>()
        {
            return (T)CreateDependencyInstanceObjects(typeof(T));
        }
        public Object CreateDependencyInstanceObjects(Type type)
        {
            // get dependency
            Dependency dependency = _container.GetDependency(type);
            //get paramaters list  of dependency type constructor
            var parameters = dependency.Type.GetConstructors().Single().GetParameters().ToArray();
            //create a list of parameter instances
            var parameterInstances = new object[parameters.Length];
            // istantiate parameters
            //base case will go untill constructor with zero parameters which is default constructor 
            if (parameters.Length > 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    //call create dependecies reccursively cause dependency can have it's dependencies
                    parameterInstances[i] = CreateDependencyInstanceObjects(parameters[i].ParameterType);
                }
                return CreateImplementation(dependency, t => Activator.CreateInstance(t, parameterInstances));
            }

            // create instance of a type
            return CreateImplementation(dependency, t => Activator.CreateInstance(t));
        }
        public Object CreateImplementation(Dependency dependency, Func<Type, Object> instantioator)
        {
            if (dependency.IsImplemented)
            {
                return dependency.Implementation;
            }
            var implementation = instantioator(dependency.Type);
            if (dependency.LifeTime == DependencyLifetimes.singleton)
            {
                dependency.AddImplementation(implementation);
            }
            return implementation;
        }
    }
    //create a dependency injection container
    public class DependencyInjectContainer
    {
        public List<Dependency> _dependencies;
        public DependencyInjectContainer()
        {
            _dependencies = new List<Dependency>();
        }
        //create a method to add singleton service
        public void AddSingleton<t>()
        {
            _dependencies.Add(new Dependency(typeof(t), DependencyLifetimes.singleton));
        }
        //create a method to add transient service
        public void AddTransient<t>()
        {
            _dependencies.Add(new Dependency(typeof(t), DependencyLifetimes.transient));
        }
        public Dependency GetDependency(Type type)
        {
            return _dependencies.First(x => x.Type.Name == type.Name);
        }
    }

    // create a class which have dpendency with other class

    public class YFunction
    {
        private X _x;
        public YFunction(X x)
        {
            _x = x;
        }

        public void SayHello()
        {
            var number = _x.GetNumber();
            Console.WriteLine($"Hello {number}");
        }
    }
    public class X
    {
        private Z _z;
        public X(Z z)
        {
            _z = z;
        }
        public int GetNumber()
        {
            return _z.GetNumber();
        }
    }
    public class Z
    {
        int _random;
        public Z()
        {
            _random = new Random().Next();
        }
        public int GetNumber()
        {
            return _random;
        }
    }

}