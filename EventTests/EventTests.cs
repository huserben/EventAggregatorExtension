using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParallelAwarePubSubEvent;
using Prism.Events;

namespace EventTests
{
   [TestClass]
   public class EventTests
   {
      public TestContext TestContext { get; set; }

      [TestMethod]
      public void SequentialEventTest()
      {
         var eventAggregator = new EventAggregator();

         for (var i = 0; i < 1_000_000; i++)
         {
            eventAggregator.GetEvent<NonParallelEvent<int>>().Subscribe(EventAction, ThreadOption.PublisherThread, false, EventFilter);
         }

         var watch = Stopwatch.StartNew();
         TestContext.WriteLine($"Start publishing events.");

         eventAggregator.GetEvent<NonParallelEvent<int>>().Publish(14);
         eventAggregator.GetEvent<NonParallelEvent<int>>().Publish(42);
         eventAggregator.GetEvent<NonParallelEvent<int>>().Publish(37);

         watch.Stop();
         var elapsedMs = watch.ElapsedMilliseconds;
         TestContext.WriteLine($"Elapsed Time in ms: {elapsedMs}");
      }

      [TestMethod]
      public void ParallelEventTest()
      {
         var eventAggregator = new EventAggregator();

         for (var i = 0; i < 1_000_000; i++)
         {
            eventAggregator.GetEvent<ParallelFilterPubSubEvent<int>>().Subscribe(EventAction, EventFilter);
         }

         var watch = Stopwatch.StartNew();
         TestContext.WriteLine($"Start publishing events.");

         eventAggregator.GetEvent<ParallelFilterPubSubEvent<int>>().Publish(14);
         eventAggregator.GetEvent<ParallelFilterPubSubEvent<int>>().Publish(42);
         eventAggregator.GetEvent<ParallelFilterPubSubEvent<int>>().Publish(37);

         watch.Stop();
         var elapsedMs = watch.ElapsedMilliseconds;
         TestContext.WriteLine($"Elapsed Time in ms: {elapsedMs}");
      }

      private void EventAction(int intValue)
      {
      }

      private bool EventFilter(int intValue)
      {
         return intValue == 42;
      }
   }

   public class NonParallelEvent<T> : PubSubEvent<T>
   {
   }
}
