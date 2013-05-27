namespace MvcBootstrap.Tests.Binding
{
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web.Mvc;

    using MvcBootstrap.ViewModels;
    using MvcBootstrap.Web.Mvc.ModelBinding;

    using NUnit.Framework;

    [TestFixture]
    public class ChoiceBindingTests
    {
        [Test]
        public void BindingEmptyValuesReturnsEmptyChoice()
        {
            //// Arrange
            
            var modelBinder = new MvcBootstrapModelBinder();
            var controllerContext = new ControllerContext();

            var formCollection = new NameValueCollection 
            { 
                // Nothing
            };

            var valueProvider = new NameValueCollectionValueProvider(formCollection, null);
            var modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(Choice<TestEntityViewModel>));

            var bindingContext = new ModelBindingContext
            {
                ModelName = string.Empty,
                ValueProvider = valueProvider,
                ModelMetadata = modelMetadata
            };

            //// Act
            
            var binding = modelBinder.BindModel(controllerContext, bindingContext) as Choice<TestEntityViewModel>;

            //// Assert
            
            Assert.That(binding, Is.Not.Null);
            Assert.That(binding.Selection, Is.Null);
        }

        [Test]
        public void BindingNonEmptyValuesReturnsCorrectChoice()
        {
            //// Arrange
            
            var modelBinder = new MvcBootstrapModelBinder();
            var controllerContext = new ControllerContext();

            int id = 1;
            var formCollection = new NameValueCollection 
            { 
                { "Choice", id.ToString(CultureInfo.InvariantCulture) }
            };

            var valueProvider = new NameValueCollectionValueProvider(formCollection, null);
            var modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(Choice<TestEntityViewModel>));

            var bindingContext = new ModelBindingContext
            {
                ModelName = "Choice",
                ValueProvider = valueProvider,
                ModelMetadata = modelMetadata
            };

            //// Act
            
            var binding = modelBinder.BindModel(controllerContext, bindingContext) as Choice<TestEntityViewModel>;

            //// Assert

            Assert.That(binding, Is.Not.Null);
            Assert.That(binding.Selection, Is.Not.Null);
            Assert.That(binding.Selection.Id, Is.EqualTo(id));
        }
    }
}
