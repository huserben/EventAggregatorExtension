using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Events;

namespace ParallelAwarePubSubEvent
{
   public class ParallelFilterPubSubEvent<T> : PubSubEvent<T>
   {
      public SubscriptionToken Subscribe(Action<T> action, Predicate<T> filter)
      {
         return Subscribe(action, ThreadOption.PublisherThread, false, filter);
      }

      protected override void InternalPublish(params object[] arguments)
      {
         var eventSubscriptionsToPublish = new List<EventSubscription<T>>();
         var lockObject = new object();

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
               lock (lockObject)
               {
                  eventSubscriptionsToPublish.Add(subscription);
               }
            }
         });

         // Execute event sequential.
         foreach (var subscriptionToPublish in eventSubscriptionsToPublish)
         {
            subscriptionToPublish.InvokeAction(subscriptionToPublish.Action, argument);
         }
      }
   }
}
