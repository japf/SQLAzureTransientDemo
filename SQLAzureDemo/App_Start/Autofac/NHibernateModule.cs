﻿using Autofac;
using Microsoft.WindowsAzure.Storage;
using NHibernate;
using NHibernate.Driver;
using SQLAzureDemo.App_Start.NHibernate;
using Autofac.Integration.Mvc;

namespace SQLAzureDemo.App_Start.Autofac
{
    public class NHibernateModule : Module
    {
        public const string TransientConnection = "nhtransient";
        public const string ResilientConnection = "nhresilient";

        private readonly string _connectionString;

        public NHibernateModule(string connectionString, CloudStorageAccount azureStorage)
        {
            _connectionString = connectionString;
            LoggingSqlAzureClientDriverWithTimeoutRetries.Initialise(azureStorage);
        }

        protected override void Load(ContainerBuilder builder)
        {
            Register<Sql2008ClientDriver>(builder, TransientConnection);
            Register<LoggingSqlAzureClientDriverWithTimeoutRetries>(builder, ResilientConnection);
        }

        private void Register<TDriver>(ContainerBuilder builder, string key)
            where TDriver: IDriver
        {
            builder.Register(c => new NHibernateConfiguration<TDriver>(_connectionString).GetSessionFactory())
                .Keyed<ISessionFactory>(key)
                .SingleInstance();

            builder.Register(c => c.ResolveKeyed<ISessionFactory>(key).OpenSession())
                .Keyed<ISession>(key)
                .InstancePerHttpRequest();
        }
    }
}