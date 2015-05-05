using System;
using System.Runtime.InteropServices.WindowsRuntime;
using IoCSharp.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IoCSharp.Test
{
    [TestClass]
    public class ContainerTest
    {

        #region Test Classes

        public interface ILogger
        {
        }

        public class SqlServerLogger : ILogger
        {
        }

        public interface IRepository<T>
        {
            ILogger Logger { get; set; }
        }

        public class SqlRepository<T> : IRepository<T>
        {
            public SqlRepository(ILogger iLogger)
            {
                Logger = iLogger;
            }

            public ILogger Logger { get; set; }
        }

        public class InvoiceService
        {
            public IRepository<Employee> Repository { get; set; }
            private ILogger Logger { get; set; }

            public InvoiceService(IRepository<Employee> repository, ILogger logger)
            {
                this.Repository = repository;
                this.Logger = logger; 
            }
        }

        public class Employee
        {
        }

        #endregion

        #region Simple Type Tests
        [TestMethod]
        public void Can_Resolve_Simple_Types_Singleton()
        {

            var ioc = new Container();
            ioc.For<ILogger>().Use<SqlServerLogger>().IsSingleton();

            var logger = ioc.Resolve<ILogger>(); 
            Assert.AreEqual(typeof(SqlServerLogger), logger.GetType());
        }

        [TestMethod]
        public void Can_Resolve_Simple_Types_Prototype()
        {
            var ioc = new Container();
            ioc.For<ILogger>().Use<SqlServerLogger>().IsPrototype();

            var logger1 = ioc.Resolve<ILogger>();
            var logger2 = ioc.Resolve<ILogger>();
            Assert.AreEqual(typeof(SqlServerLogger), logger1.GetType());
            Assert.AreEqual(typeof(SqlServerLogger), logger2.GetType());
            Assert.IsFalse(logger1 == logger2);
        }
        #endregion

        #region Unresolvable Types 
        [TestMethod]
        [ExpectedException(typeof(IoCSharpException))]
        public void Throws_Exception_When_Type_Is_Not_Configured()
        {
            var ioc = new Container();
            ioc.For<IRepository<Employee>>().Use<SqlRepository<Employee>>().IsSingleton();
            ioc.Resolve<ILogger>();
        }
        #endregion

        #region Constructor Tests
        [TestMethod]
        public void Can_Resolve_Types_With_Non_Default_Constructor_Singleton()
        {

            var ioc = new Container();
            ioc.For<IRepository<Employee>>().Use <SqlRepository<Employee>>().IsSingleton();
            ioc.For<ILogger>().Use<SqlServerLogger>().IsSingleton();

            var repository = ioc.Resolve<IRepository<Employee>>();
            Assert.AreEqual(typeof(SqlRepository<Employee>), repository.GetType());
        }

        [TestMethod]
        public void Can_Resolve_Types_With_Non_Default_Constructor_Prototype()
        {

            var ioc = new Container();
            ioc.For<IRepository<Employee>>().Use<SqlRepository<Employee>>().IsSingleton();
            ioc.For<ILogger>().Use<SqlServerLogger>().IsSingleton();

            var repository1 = ioc.Resolve<IRepository<Employee>>();
            var repository2 = ioc.Resolve<IRepository<Employee>>();
            Assert.AreEqual(typeof(SqlRepository<Employee>), repository1.GetType());
            Assert.AreEqual(typeof(SqlRepository<Employee>), repository2.GetType());
            Assert.IsTrue(repository1 == repository2);

            Assert.AreEqual(typeof(SqlServerLogger), repository1.Logger.GetType());
            Assert.AreEqual(typeof(SqlServerLogger), repository2.Logger.GetType());
            Assert.IsTrue(repository1.Logger == repository2.Logger);
        }

        [TestMethod]
        public void Can_Resolve_Types_With_Non_Default_Constructor_Prototype_With_Inner_Type_Prototye()
        {

            var ioc = new Container();
            ioc.For<IRepository<Employee>>().Use<SqlRepository<Employee>>().IsPrototype();
            ioc.For<ILogger>().Use<SqlServerLogger>().IsPrototype();

            var repository1 = ioc.Resolve<IRepository<Employee>>();
            var repository2 = ioc.Resolve<IRepository<Employee>>();
            Assert.AreEqual(typeof(SqlRepository<Employee>), repository1.GetType());
            Assert.AreEqual(typeof(SqlRepository<Employee>), repository2.GetType());
            Assert.IsFalse(repository1 == repository2);

            Assert.AreEqual(typeof(SqlServerLogger), repository1.Logger.GetType());
            Assert.AreEqual(typeof(SqlServerLogger), repository2.Logger.GetType());
            Assert.IsFalse(repository1.Logger == repository2.Logger);
        }
        #endregion

        #region Resolve With Dependencies 
        [TestMethod]
        public void Can_Resolve_Non_Configured_Types_With_Satisfied_Dependencies()
        {
            var ioc = new Container();
            ioc.For<IRepository<Employee>>().Use<SqlRepository<Employee>>().IsPrototype();
            ioc.For<ILogger>().Use<SqlServerLogger>().IsPrototype();
            InvoiceService invoidService = ioc.Resolve<InvoiceService>();
            Assert.AreEqual(typeof(InvoiceService), invoidService.GetType());
        }
        #endregion

        #region Open Type Tests
        [TestMethod]
        public void Can_Resolve_Open_Types_Singleton()
        {

            var ioc = new Container();
            ioc.For(typeof(IRepository<>)).Use(typeof(SqlRepository<>)).IsSingleton();
            ioc.For<ILogger>().Use<SqlServerLogger>().IsSingleton();

            var repository1 = ioc.Resolve<IRepository<Employee>>();
            var repository2 = ioc.Resolve<IRepository<Employee>>();
            Assert.AreEqual(typeof(SqlRepository<Employee>), repository1.GetType());
            Assert.AreEqual(typeof(SqlRepository<Employee>), repository2.GetType());
            Assert.IsTrue(repository1 == repository2);
        }

        [TestMethod]
        public void Can_Resolve_Open_Types_Prototype()
        {

            var ioc = new Container();
            ioc.For(typeof(IRepository<>)).Use(typeof(SqlRepository<>)).IsPrototype();
            ioc.For<ILogger>().Use<SqlServerLogger>().IsSingleton();

            var repository1 = ioc.Resolve<IRepository<Employee>>();
            var repository2 = ioc.Resolve<IRepository<Employee>>();
            Assert.AreEqual(typeof(SqlRepository<Employee>), repository1.GetType());
            Assert.AreEqual(typeof(SqlRepository<Employee>), repository2.GetType());
            Assert.IsFalse(repository1 == repository2);
        }
        #endregion
    }
}
