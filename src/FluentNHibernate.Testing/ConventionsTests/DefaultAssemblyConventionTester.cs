﻿using System.Collections.Generic;
using FluentNHibernate.Mapping;
using FluentNHibernate.Mapping.Conventions;
using NUnit.Framework;
using Rhino.Mocks;

namespace FluentNHibernate.Testing.ConventionsTests
{
    [TestFixture]
    public class DefaultAssemblyConventionTester
    {
        private readonly List<IClassMap> EmptyList = new List<IClassMap>();
        private DefaultAssemblyConvention convention;
        private IConventionFinder conventionFinder;
        private readonly ConventionOverrides Overrides = new ConventionOverrides();

        [SetUp]
        public void CreateConvention()
        {
            conventionFinder = MockRepository.GenerateMock<IConventionFinder>();
            convention = new DefaultAssemblyConvention(conventionFinder);
        }

        [Test]
        public void ApplyFindsConventions()
        {
            convention.Apply(EmptyList, Overrides);
            conventionFinder.AssertWasCalled(x => x.Find<IClassConvention>());
        }

        [Test]
        public void ApplyCallsAcceptOnAllConventions()
        {
            var conventions = new[]
            {
                MockRepository.GenerateMock<IClassConvention>(),
                MockRepository.GenerateMock<IClassConvention>()
            };
            var classes = new[]
            {
                MockRepository.GenerateStub<IClassMap>(),
                MockRepository.GenerateStub<IClassMap>(),
                MockRepository.GenerateStub<IClassMap>()
            };

            conventionFinder.Stub(x => x.Find<IClassConvention>())
                .Return(conventions);

            convention.Apply(classes, new ConventionOverrides());

            // each convention gets Accept called for each class
            conventions[0].AssertWasCalled(x => x.Accept(classes[0]));
            conventions[0].AssertWasCalled(x => x.Accept(classes[1]));
            conventions[0].AssertWasCalled(x => x.Accept(classes[2]));
            conventions[1].AssertWasCalled(x => x.Accept(classes[0]));
            conventions[1].AssertWasCalled(x => x.Accept(classes[1]));
            conventions[1].AssertWasCalled(x => x.Accept(classes[2]));
        }

        [Test]
        public void ApplyAppliesAllAcceptedConventions()
        {
            var conventions = new[]
            {
                MockRepository.GenerateMock<IClassConvention>(),
                MockRepository.GenerateMock<IClassConvention>()
            };
            var classes = new[] { MockRepository.GenerateStub<IClassMap>() };

            conventionFinder.Stub(x => x.Find<IClassConvention>())
                .Return(conventions);

            conventions[0].Stub(x => x.Accept(classes[0])).Return(true);
            conventions[1].Stub(x => x.Accept(classes[0])).Return(false);

            convention.Apply(classes, Overrides);

            // each convention gets Apply called for any classes it returned true for Accept
            conventions[0].AssertWasCalled(x => x.Apply(classes[0], Overrides));
            conventions[1].AssertWasNotCalled(x => x.Apply(classes[0], Overrides));
        }
    }
}