﻿using AMT.Data.Common;
using AMT.Data.Entities;
using GraphQL;
using GraphQL.Http;
using GraphQLApi.Contracts;
using GraphQLApi.GraphQL.Queries;
using GraphQLApi.GraphQL.Schema;
using GraphQLApi.GraphQL.Types;
using GraphQLApi.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GraphQLApi.Tests
{
    public class TestFixture 
    {
        private readonly DocumentExecuter DocumentExecutor;
        private readonly AppSchema AppSchema;
        public IServiceProvider ServiceProvider { get; set; }

        public TestFixture()
        {
            DocumentExecutor = new DocumentExecuter();
            var serviceCollection = new ServiceCollection();
            SetupGraphQLApi(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
            AppSchema = ServiceProvider.GetRequiredService<AppSchema>();

        }

        private void SetupGraphQLApi(ServiceCollection services)
        {
            services.AddDbContext<ODSContext>(opt => opt.UseSqlServer("Data Source = localhost; Initial Catalog = GDale_EdFi; Integrated Security = True; MultipleActiveResultSets = True"));

            services.AddScoped<IStudentInformationRepository, StudentInformationRepository>();
            services.AddScoped<IStudentMetricCalculator, StudentAttendanceRepository>();

            services.AddScoped<IDependencyResolver>(s => new FuncDependencyResolver(s.GetRequiredService));
            services.AddScoped<AppSchema>();
            services.AddScoped<AppQuery>();
            services.AddScoped<StudentInformationType>();
            services.AddScoped<StudentIndicatorType>();
            services.AddScoped<StudentParentInformationType>();
            services.AddScoped<StudentSchoolInformationType>();
            services.AddScoped<StudentMetricType>();
        }

        public async Task<string> QueryGraphQLAsync(string query)
        {
            var result = await DocumentExecutor
                .ExecuteAsync(options =>
                {
                    options.Schema = AppSchema;
                    options.Query = query;
                })
                .ConfigureAwait(false);

            var json = new DocumentWriter(indent: true).Write(result);
            return json;
        }

    }
}
