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
      private int filterCounter;
      private int actionCounter;

      private object lockObject = new object();

      public TestContext TestContext { get; set; }

      [TestInitialize]
      public void Setup()
      {
         filterCounter = 0;
         actionCounter = 0;
      }

      [TestMethod]
      public void SequentialEventTest()
      {
         var eventAggregator = new EventAggregator();

         for (var i = 0; i < 1_000_000; i++)
         {
            eventAggregator.GetEvent<NonParallelEvent<int>>().Subscribe(EventAction, ThreadOption.PublisherThread, false, EventFilter);
         }

         eventAggregator.GetEvent<NonParallelEvent<int>>().Subscribe(EventAction, ThreadOption.PublisherThread, false, intValue => intValue == 42);

         var watch = Stopwatch.StartNew();
         TestContext.WriteLine($"Start publishing events.");

         eventAggregator.GetEvent<NonParallelEvent<int>>().Publish(42);

         watch.Stop();
         var elapsedMs = watch.ElapsedMilliseconds;
         TestContext.WriteLine($"Elapsed Time in ms: {elapsedMs}");
         TestContext.WriteLine($"FilterCounter: {filterCounter}, ActionCounter: {actionCounter}");
      }

      [TestMethod]
      public void ParallelFilterEventTest()
      {
         var eventAggregator = new EventAggregator();

         for (var i = 0; i < 1_000_000; i++)
         {
            eventAggregator.GetEvent<ParallelFilterPubSubEvent<int>>().Subscribe(EventAction, EventFilter);
         }

         eventAggregator.GetEvent<ParallelFilterPubSubEvent<int>>().Subscribe(EventAction, ThreadOption.PublisherThread, false, intValue => intValue == 42);

         var watch = Stopwatch.StartNew();
         TestContext.WriteLine($"Start publishing events.");

         eventAggregator.GetEvent<ParallelFilterPubSubEvent<int>>().Publish(42);

         watch.Stop();
         var elapsedMs = watch.ElapsedMilliseconds;
         TestContext.WriteLine($"Elapsed Time in ms: {elapsedMs}");
         TestContext.WriteLine($"FilterCounter: {filterCounter}, ActionCounter: {actionCounter}");
      }

      [TestMethod]
      public void CompleteParallelEventTest()
      {
         var eventAggregator = new EventAggregator();

         for (var i = 0; i < 1_000_000; i++)
         {
            eventAggregator.GetEvent<ParallelPubSubEvent<int>>().Subscribe(EventAction, EventFilter);
         }

         eventAggregator.GetEvent<ParallelPubSubEvent<int>>().Subscribe(EventAction, ThreadOption.PublisherThread, false, intValue => intValue == 42);

         var watch = Stopwatch.StartNew();
         TestContext.WriteLine($"Start publishing events.");

         eventAggregator.GetEvent<ParallelPubSubEvent<int>>().Publish(42);

         watch.Stop();
         var elapsedMs = watch.ElapsedMilliseconds;
         TestContext.WriteLine($"Elapsed Time in ms: {elapsedMs}");
         TestContext.WriteLine($"FilterCounter: {filterCounter}, ActionCounter: {actionCounter}");
      }

      [TestMethod]
      public void CompleteParallelOnBackgroundThreadEventTest()
      {
         var eventAggregator = new EventAggregator();

         for (var i = 0; i < 1_000_000; i++)
         {
            eventAggregator.GetEvent<ParallelPubSubEvent<int>>().Subscribe(EventAction, EventFilter);
         }

         eventAggregator.GetEvent<ParallelPubSubEvent<int>>().Subscribe(EventAction, ThreadOption.BackgroundThread, false, intValue => intValue == 42);

         var watch = Stopwatch.StartNew();
         TestContext.WriteLine($"Start publishing events.");

         eventAggregator.GetEvent<ParallelPubSubEvent<int>>().Publish(42);

         watch.Stop();
         var elapsedMs = watch.ElapsedMilliseconds;
         TestContext.WriteLine($"Elapsed Time in ms: {elapsedMs}");
         TestContext.WriteLine($"FilterCounter: {filterCounter}, ActionCounter: {actionCounter}");
      }

      private void EventAction(int intValue)
      {
         actionCounter++;
         TestContext.WriteLine("We got a hit");
      }

      private bool EventFilter(int intValue)
      {
         var random = new Random();

         lock (lockObject)
         {
            filterCounter++;
         }

         return intValue == random.Next();
      }
   }

   public class NonParallelEvent<T> : PubSubEvent<T>
   {
   }
}
