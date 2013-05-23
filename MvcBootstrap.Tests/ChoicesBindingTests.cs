namespace MvcBootstrap.Tests
{
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using MvcBootstrap.ViewModels;
    using MvcBootstrap.Web.Mvc.ModelBinding;

    using NUnit.Framework;

    [TestFixture]
    public class ChoicesBindingTests
    {
        [Test]
        public void BindingEmptyValuesReturnsEmptyChoices()
        {
            //// Arrange

            var modelBinder = new MvcBootstrapModelBinder();
            var controllerContext = new ControllerContext();

            var formCollection = new NameValueCollection
            {
                // Nothing
            };

            var valueProvider = new NameValueCollectionValueProvider(formCollection, null);
            var modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(Choices<EntityViewModel>));

            var bindingContext = new ModelBindingContext
            {
                ModelName = string.Empty,
                ValueProvider = valueProvider,
                ModelMetadata = modelMetadata
            };

            //// Act

            var binding = modelBinder.BindModel(controllerContext, bindingContext) as Choices<EntityViewModel>;

            //// Assert

            Assert.That(binding, Is.Not.Null);
            Assert.That(binding.Selections, Is.Not.Null);
            Assert.That(binding.Selections, Is.Empty);
        }

        [Test]
        public void BindingNonEmptyResultsReturnsCorrectChoices()
        {
            //// Arrange

            var modelBinder = new MvcBootstrapModelBinder();
            var controllerContext = new ControllerContext();

            var ids = new[] { 1, 2, 3 };
            var formCollection = new NameValueCollection 
            { 
                { "Choices.Id", ids[0].ToString(CultureInfo.InvariantCulture) },
                { "Choices.Id", ids[1].ToString(CultureInfo.InvariantCulture) },
                { "Choices.Id", ids[2].ToString(CultureInfo.InvariantCulture) }
            };

            var valueProvider = new NameValueCollectionValueProvider(formCollection, null);
            var modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(Choices<EntityViewModel>));

            var bindingContext = new ModelBindingContext
            {
                ModelName = "Choices",
                ValueProvider = valueProvider,
                ModelMetadata = modelMetadata
            };

            //// Act

            var binding = modelBinder.BindModel(controllerContext, bindingContext) as Choices<EntityViewModel>;

            //// Assert

            Assert.That(binding, Is.Not.Null);
            Assert.That(binding.Selections, Is.Not.Null);
            Assert.That(binding.Selections, Is.Not.Empty);
            Assert.That(binding.Selections.Select(vm => vm.Id), Is.EqualTo(ids));
        }
    }
}
