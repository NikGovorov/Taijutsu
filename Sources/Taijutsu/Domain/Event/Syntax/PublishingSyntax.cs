using System;

namespace Taijutsu.Domain.Event.Syntax
{
    public class PublishingSyntax
    {
        private readonly Action publishAction;

        public PublishingSyntax(Action publishAction)
        {
            this.publishAction = publishAction;
        }

        public virtual void Publish()
        {
            publishAction();
        }
    }
}