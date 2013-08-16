namespace Taijutsu.Domain
{
    public interface ICompositeSpecification<TDomainObject> : ISpecification<TDomainObject> where TDomainObject : IDomainObject
    {
        ISpecification<TDomainObject> One { get; }

        ISpecification<TDomainObject> Other { get; }
    }
}