using NSubstitute;
using NUnit.Framework;
using Taijutsu.Domain;
using Taijutsu.Test.Domain.Model;

namespace Taijutsu.Test.Domain
{
    [TestFixture]
    public class DeletableEntityExFixture
    {
        [Test]
        public virtual void ShouldCallAppropriateUnitOfWorkMethod()
        {
            var uow = Substitute.For<IUnitOfWork>();
            var customer = new Customer(SeqGuid.NewGuid(), new FullName("Test", "Test"));
            customer.AsDeletedIn(uow);
            uow.Received(1).MarkAsDeleted(customer as IDeletableEntity);
        }
    }
}