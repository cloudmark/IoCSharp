# CS IoC Container 

This is a small prototype how to create a simple IoC container.  The usage of the container is as follows: 

For closed types you can use the container as follows: 

    var ioc = new Container();
    ioc.For<IRepository<Employee>>().Use<SqlRepository<Employee>>().IsPrototype();
    ioc.For<ILogger>().Use<SqlServerLogger>().IsPrototype();
    var repository1 = ioc.Resolve<IRepository<Employee>>();
	var repository2 = ioc.Resolve<IRepository<Employee>>();

For open types you can use the container as follows: 

    var ioc = new Container();
    ioc.For(typeof(IRepository<>)).Use(typeof(SqlRepository<>)).IsPrototype();
	ioc.For<ILogger>().Use<SqlServerLogger>().IsSingleton();
    var repository1 = ioc.Resolve<IRepository<Employee>>();

You should not use this IoC container in production.  This is only meant as an experiment.  