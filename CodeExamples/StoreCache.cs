using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ploeh.Samples.Encapsulation.CodeExamples
{
    public class StoreCache : IStoreWriter, IStoreReader
    {
        private readonly ConcurrentDictionary<int, Maybe<string>> cache;
        private readonly IStoreWriter writer;
        private readonly IStoreReader reader;

        public StoreCache(IStoreWriter writer, IStoreReader reader)
        {
            this.cache = new ConcurrentDictionary<int, Maybe<string>>();
            this.writer = writer;
            this.reader = reader;
        }

        public void Save(int id, string message)
        {
            this.writer.Save(id, message);
            var m = new Maybe<string>(message);
            this.cache.AddOrUpdate(id, m, (i, s) => m);
        }

        public Maybe<string> Read(int id)
        {
            Maybe<string> retVal;
            if (this.cache.TryGetValue(id, out retVal))
                return retVal;

            retVal = this.reader.Read(id);
            if (retVal.Any())
                this.cache.AddOrUpdate(id, retVal, (i, s) => retVal);

            return retVal;
        }
    }
}
