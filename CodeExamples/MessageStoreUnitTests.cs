using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture.Xunit;
using Serilog;
using Serilog.Events;
using Xunit;
using Xunit.Extensions;

namespace Ploeh.Samples.Encapsulation.CodeExamples
{
    public class MessageStoreUnitTests
    {
        [Theory, AutoData]
        public void ReadReturnsMessage(string expected)
        {
            var msgStore = CreateMessageStore();
            msgStore.Save(44, expected);

            var actual = msgStore.Read(44);

            Assert.Equal(expected, actual.Single());
        }

        [Theory, AutoData]
        public void GetFileFileReturnsCorrectResult(int id)
        {
            var workingDirectory = new DirectoryInfo(Environment.CurrentDirectory);
            var msgStore = CreateMessageStore();

            var actual = msgStore.GetFileInfo(id).FullName;

            var expected =
                Path.Combine(workingDirectory.FullName, id + ".txt");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ConstructWithNullDirectoryThrows()
        {
            Assert.Throws<ArgumentNullException>(
                () => new MessageStore(
                    new FileStore(null),
                    new FileStore(null),
                    new FileStore(null)));
        }

        [Theory, AutoData]
        public void ConstructWithInvalidDirectoryThrows(string invalidDirectory)
        {
            Assert.False(Directory.Exists(invalidDirectory));
            Assert.Throws<ArgumentException>(
                () => new MessageStore(
                    new FileStore(new DirectoryInfo(invalidDirectory)),
                    new FileStore(new DirectoryInfo(invalidDirectory)),
                    new FileStore(new DirectoryInfo(invalidDirectory))));
        }

        [Theory, AutoData]
        public void ReadUsageExample(string expected)
        {
            var msgStore = CreateMessageStore();
            msgStore.Save(49, expected);

            var message = msgStore.Read(49).DefaultIfEmpty("").Single();

            Assert.Equal(expected, message);
        }

        [Theory, AutoData]
        public void ReadExistingFileReturnsTrue(string expected)
        {
            var msgStore = CreateMessageStore();
            msgStore.Save(50, expected);

            var actual = msgStore.Read(50);

            Assert.True(actual.Any());
            Assert.Equal(expected, actual.Single());
        }

        [Theory, AutoData]
        public void ReadNonExistingFileReturnsFalse(string expected)
        {
            var msgStore = CreateMessageStore();

            var actual = msgStore.Read(51);

            Assert.False(actual.Any());
        }

        [Theory, AutoData]
        public void SaveLogsInformation(string message)
        {
            var spy = new SpySink();
            var logger = new LoggerConfiguration().WriteTo.Sink(spy).CreateLogger();
            var msgStore = CreateMessageStore(logger);

            msgStore.Save(52, message);

            Assert.True(spy.Events
                .SelectMany(le => le.Properties)
                .Where(kvp => kvp.Key == "id")
                .Select(kvp => kvp.Value)
                .OfType<ScalarValue>()
                .Any(sv => sv.Value.Equals(52)));
        }

        [Theory, AutoData]
        public void ReadExistingMessageLogsCorrectDebugInformation(string message)
        {
            var msgStore = CreateMessageStore();
            msgStore.Save(53, message);
            var spy = new SpySink();
            var logger = new LoggerConfiguration().WriteTo.Sink(spy).MinimumLevel.Debug().CreateLogger();
            msgStore = CreateMessageStore(logger);

            msgStore.Read(53);

            Assert.True(spy.Events
                .Where(le => le.MessageTemplate.Text == "Reading message {id}.")
                .SelectMany(le => le.Properties)
                .Where(kvp => kvp.Key == "id")
                .Select(kvp => kvp.Value)
                .OfType<ScalarValue>()
                .Any(sv => sv.Value.Equals(53)));
            Assert.True(spy.Events
                .Where(le => le.MessageTemplate.Text == "Returning message {id}.")
                .SelectMany(le => le.Properties)
                .Where(kvp => kvp.Key == "id")
                .Select(kvp => kvp.Value)
                .OfType<ScalarValue>()
                .Any(sv => sv.Value.Equals(53)));
        }

        [Fact]
        public void ReadNonExistingMessageLogsCorrectDebugInformation()
        {
            var spy = new SpySink();
            var logger = new LoggerConfiguration().WriteTo.Sink(spy).MinimumLevel.Debug().CreateLogger();
            var msgStore = CreateMessageStore(logger);

            msgStore.Read(54);

            Assert.True(spy.Events
                .Where(le => le.MessageTemplate.Text == "No message {id} found.")
                .SelectMany(le => le.Properties)
                .Where(kvp => kvp.Key == "id")
                .Select(kvp => kvp.Value)
                .OfType<ScalarValue>()
                .Any(sv => sv.Value.Equals(54)));
        }

        [Theory, AutoData]
        public void ReadReadsFromCache(
            string shouldBeCached,
            string backDoorMessage)
        {
            var msgStore = CreateMessageStore();
            msgStore.Save(55, shouldBeCached);
            msgStore.Read(55);
            var file = msgStore.GetFileInfo(55);
            File.WriteAllText(file.FullName, backDoorMessage);

            var actual = msgStore.Read(55);

            Assert.Equal(shouldBeCached, actual.Single());
        }

        [Theory, AutoData]
        public void SaveShouldInvalidateCache(string message1, string expected)
        {
            var msgStore = CreateMessageStore();
            msgStore.Save(56, message1);
            msgStore.Read(56);
            msgStore.Save(56, expected);

            var actual = msgStore.Read(56);

            Assert.Equal(expected, actual.Single());
        }

        [Theory, AutoData]
        public void ReadReadsThroughOnCacheMiss(string expected)
        {
            var msgStore = CreateMessageStore();
            var file = msgStore.GetFileInfo(57);
            File.WriteAllText(file.FullName, expected);

            var actual = msgStore.Read(57);

            Assert.NotEmpty(actual);
            Assert.Equal(expected, actual.Single());
        }

        private static MessageStore CreateMessageStore()
        {
            var logger = new LoggerConfiguration().CreateLogger();
            return CreateMessageStore(logger);
        }

        private static MessageStore CreateMessageStore(ILogger logger)
        {
            var fileStore =
                new FileStore(
                    new DirectoryInfo(
                        Environment.CurrentDirectory));
            var cache = new StoreCache(fileStore, fileStore);
            var log = new StoreLogger(logger, cache, cache);
            var msgStore = new MessageStore(
                log,
                log,
                fileStore);
            return msgStore;
        }
    }
}
