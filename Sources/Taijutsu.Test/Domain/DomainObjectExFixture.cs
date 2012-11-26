using NSubstitute;
using NUnit.Framework;
using Taijutsu.Domain;
using Taijutsu.Test.Domain.Model;

namespace Taijutsu.Test.Domain
{
    [TestFixture]
    public class DomainObjectExFixture
    {
        [Test]
        public virtual void ShouldCallAppropriateSpecificationMethod()
        {
            var spec = Substitute.For<ISpecification>();
            var customer = new Customer(SeqGuid.NewGuid(), new FullName("Test", "Test"));
            customer.Satisfies(spec);
            spec.Received(1).IsSatisfiedBy(customer);
        }
    }
}