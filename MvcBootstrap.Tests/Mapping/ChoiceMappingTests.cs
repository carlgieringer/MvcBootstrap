namespace MvcBootstrap.Tests.Mapping
{
    using System.Linq;

    using AutoMapper;

    using Moq;

    using MvcBootstrap.Data;
    using MvcBootstrap.Web.Mvc.Controllers;
    using MvcBootstrap.Web.Mvc.Controllers.FluentConfig;

    using NUnit.Framework;

    [TestFixture]
    public class ChoiceMappingTests
    {
        private SelfReferentialEntity Entity { get; set; }

        private RelationsConfig<SelfReferentialEntity> RelationsConfig { get; set; }

        private IGivenOptions RelationConfig { get; set; }

        [SetUp]
        public void SetUp()
        {
            var selectedOther = new SelfReferentialEntity
            {
                Id = 2
            };
            this.Entity = new SelfReferentialEntity
            {
                Id = 1,
                Other = selectedOther
            };

            var repository = new Mock<IBootstrapRepository<SelfReferentialEntity>>();
            repository.Setup(r => r.GetAll()).Returns(new[] { this.Entity, selectedOther });

            this.RelationsConfig = new RelationsConfig<SelfReferentialEntity>();

            this.RelationConfig = this.RelationsConfig.Relation(e => e.Other)
                .HasOptions(e => repository.Object.GetAll());

            var mappingCreator = new MappingCreator<SelfReferentialEntity>(this.RelationsConfig);
            mappingCreator.CreateEntityToViewModelMap<SelfReferentialEntity, SelfReferentialEntityViewModel>();
            mappingCreator.CreateEntityToChoiceMap<SelfReferentialEntity, SelfReferentialEntityViewModel>();
        }

        [Test]
        public void SelfReferenceMapsToInitializedChoice()
        {
            //// Act

            var viewModel = Mapper.Map<SelfReferentialEntityViewModel>(this.Entity);

            //// Assert

            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.Other, Is.Not.Null);
            Assert.That(viewModel.Other.Selection, Is.Not.Null);
        }

        [Test]
        public void SelfReferentialChoiceMapsSelection()
        {
            //// Act
            
            var viewModel = Mapper.Map<SelfReferentialEntityViewModel>(this.Entity);

            //// Assert

            Assert.That(viewModel.Other.Selection.Id, Is.EqualTo(this.Entity.Other.Id));
        }

        [Test]
        public void SelfReferentialChoiceMapsOptions()
        {
            //// Act

            var viewModel = Mapper.Map<SelfReferentialEntityViewModel>(this.Entity);

            //// Assert

            Assert.That(viewModel.Other.Options, Is.Not.Empty);
        }

        [Test]
        public void SelfReferentialChoiceMapRespectsDisallowSelfOption()
        {
            //// Arrange

            this.RelationConfig.CanChooseSelf(false);

            //// Act

            var viewModel = Mapper.Map<SelfReferentialEntityViewModel>(this.Entity);

            //// Assert

            Assert.That(viewModel.Other.Options.Select(vm => vm.Id), Has.None.EqualTo(viewModel.Id));
        }
    }
}
