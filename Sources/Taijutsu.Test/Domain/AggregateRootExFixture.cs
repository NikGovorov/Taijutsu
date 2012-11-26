using NSubstitute;
using NUnit.Framework;
using Taijutsu.Domain;
using Taijutsu.Test.Domain.Model;

namespace Taijutsu.Test.Domain
{
    [TestFixture]
    public class AggregateRootExFixture
    {
        [Test]
        public virtual void ShouldCallAppropriateUnitOfWorkMethod()
        {
            var uow = Substitute.For<IUnitOfWork>();
            var customer = new Customer(SeqGuid.NewGuid(), new FullName("Test", "Test"));
            customer.AsCreatedIn(uow);
            uow.Received(1).MarkAsCreated(customer as IAggregateRoot);
        }
    }
}