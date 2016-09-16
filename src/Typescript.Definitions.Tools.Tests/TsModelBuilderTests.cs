using System;
using System.Linq;
using System.Reflection;
using Typescript.Definitions.Tools.Tests.TestModels;
using Typescript.Definitions.Tools.TsModels;
using Xunit;

namespace Typescript.Definitions.Tools.Tests
{
    public class TsModelBuilderTests
    {

        #region Add tests

        [Fact]
        public void WhenAddTypeThatIsntClassStructOrEnum_ExceptionIsThrown()
        {
            var target = new TsModelBuilder();

            Assert.Throws<ArgumentException>(() => target.Add(typeof(string)));
        }

        [Fact]
        public void WhenAddEnum_EnumIsAddedToModel()
        {
            var target = new TsModelBuilder();
            target.Add(typeof(CustomerKind));

            Assert.Single(target.Enums.Values.Where(o => o.Type == typeof(CustomerKind)));
        }

        [Fact]
        public void WhenAdd_ClassIsAddedToModel()
        {
            var target = new TsModelBuilder();

            target.Add(typeof(Address), true);

            Assert.Single(target.Classes.Values.Where(o => o.Type == typeof(Address)));
        }

        [Fact]
        public void WhenAddAndIncludeReferencesIsFalse_ReferencedClassesAreNotAddedToModel()
        {
            var target = new TsModelBuilder();

            target.Add(typeof(Person), false);

            Assert.Single(target.Classes.Values.Where(o => o.Type == typeof(Person)));
            Assert.Empty(target.Classes.Values.Where(o => o.Type == typeof(Address)));
        }

        [Fact]
        public void WhenAddAndIncludeReferencesIsTrue_ReferencedClassesAreAddedToModel()
        {
            var target = new TsModelBuilder();

            target.Add(typeof(Person), true);

            Assert.Single(target.Classes.Values.Where(o => o.Type == typeof(Person)));
            Assert.Single(target.Classes.Values.Where(o => o.Type == typeof(Address)));
        }

        [Fact]
        public void WhenAddAndClassHasBaseClass_BaseClassIsAddedToModel()
        {
            var target = new TsModelBuilder();

            target.Add(typeof(Employee), false);

            Assert.Single(target.Classes.Values.Where(o => o.Type == typeof(Employee)));
            Assert.Single(target.Classes.Values.Where(o => o.Type == typeof(Person)));
        }

        [Fact]
        public void WhenAddClassWithReferenceAndReferenceIsCollectionOfCustomType_CustomTypeIsAddedToModel()
        {
            var target = new TsModelBuilder();

            target.Add<CustomTypeCollectionReference>(true);

            Assert.Single(target.Classes.Values.Where(o => o.Type == typeof(CustomTypeCollectionReference)));
            Assert.Single(target.Classes.Values.Where(o => o.Type == typeof(Product)));
        }

        [Fact]
        public void WhenAddClassWithReferenceAndReferenceIsIEnumerableOfCustomType_CustomTypeIsAddedToModel()
        {
            var target = new TsModelBuilder();

            target.Add<CustomTypeCollectionReference>(true);

            Assert.Single(target.Classes.Values.Where(o => o.Type == typeof(CustomTypeCollectionReference)));
            Assert.Single(target.Classes.Values.Where(o => o.Type == typeof(Person)));
        }

        [Fact]
        public void WhenInterfaceIsAdded_InterfaceIsAddedAsClass()
        {
            var target = new TsModelBuilder();

            target.Add<IShippingService>(true);

            Assert.Single(target.Classes.Values.Where(o => o.Type == typeof(IShippingService)));
        }

        #endregion

        #region Add(Assembly) tests

        [Fact]
        public void WhenAdd_AllClassesWithTsClassAttributeAreAdded()
        {
            var target = new TsModelBuilder();
            target.Add(typeof(Product).GetTypeInfo().Assembly);

            Assert.Single(target.Classes.Values.Where(o => o.Type == typeof(Product)));
        }

        #endregion

        #region Build tests

        [Fact]
        public void WhenBuild_ModelWithAddedClassesIsReturned()
        {
            var target = new TsModelBuilder();
            target.Add(typeof(Person), true);

            var model = target.Build();

            Assert.Equal(target.Classes.Values, model.Classes);
        }

        [Fact]
        public void WhenBuild_ModelWithModulesIsReturned()
        {
            var target = new TsModelBuilder();
            target.Add(typeof(Person), true);

            var model = target.Build();

            var module = model.Modules.Single(m => m.Name == "Typescript.Definitions.Tools.Tests.TestModels");
            var personClass = model.Classes.Single(o => o.Type == typeof(Person));

            Assert.Same(personClass.Module, module);
        }

        [Fact]
        public void WhenBuild_TypeReferencesInModelAreResolved()
        {
            var target = new TsModelBuilder();
            target.Add(typeof(Person), true);

            var model = target.Build();

            var personClass = model.Classes.Single(o => o.Type == typeof(Person));
            var addressClass = model.Classes.Single(o => o.Type == typeof(Address));

            Assert.Same(addressClass, personClass.Properties.Single(p => p.Name == "PrimaryAddress").PropertyType);
            Assert.IsType<TsSystemType>(personClass.Properties.Single(p => p.Name == "Name").PropertyType);
            Assert.IsType<TsCollection>(personClass.Properties.Single(p => p.Name == "Addresses").PropertyType);

            Assert.IsType<TsSystemType>(personClass.Fields.Single(f => f.Name == "PhoneNumber").PropertyType);

            Assert.IsType<TsSystemType>(personClass.Constants.Single(c => c.Name == "MaxAddresses").PropertyType);
        }

        [Fact]
        public void WhenBuild_ModulesInModelAreResolved()
        {
            var target = new TsModelBuilder();
            target.Add(typeof(Person));

            var model = target.Build();

            var personClass = model.Classes.Single(o => o.Type == typeof(Person));
            var addressClass = model.Classes.Single(o => o.Type == typeof(Address));

            Assert.Same(personClass.Module, addressClass.Module);
        }

        #endregion
    }
}