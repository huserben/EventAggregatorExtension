using System;
using System.Linq;
using System.Threading.Tasks;
using Prism.Events;

namespace ParallelAwarePubSubEvent
{
   public class ParallelPubSubEvent<T> : PubSubEvent<T>
   {
      public SubscriptionToken Subscribe(Action<T> action, Predicate<T> filter)
      {
         return Subscribe(action, ThreadOption.PublisherThread, false, filter);
      }

      public SubscriptionToken Subscribe(Action<T> action, Predicate<T> filter, ThreadOption threadOption)
      {
         return Subscribe(action, threadOption, false, filter);
      }

      protected override void InternalPublish(params object[] arguments)
      {
         var argument = default(T);
         if (arguments != null && arguments.Length > 0 && arguments[0] != null)
         {
            argument = (T)arguments[0];
         }

         Parallel.ForEach(Subscriptions.OfType<EventSubscription<T>>(), (subscription)
            =>
         {
            if (subscription.Filter == null || subscription.Filter(argument))
            {
               subscription.Action(argument);
            }
         });
      }
   }
}
