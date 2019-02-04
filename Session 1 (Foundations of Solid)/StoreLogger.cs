using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace Ploeh.Samples.Encapsulation.CodeExamples
{
    public class StoreLogger : IStoreWriter, IStoreReader
    {
        private readonly ILogger log;
        private readonly IStoreWriter writer;
        private readonly IStoreReader reader;

        public StoreLogger(ILogger log, IStoreWriter writer, IStoreReader reader)
        {
            this.log = log;
            this.writer = writer;
            this.reader = reader;
        }

        public void Save(int id, string message)
        {
            this.log.Information("Saving message {id}.", id);
            this.writer.Save(id, message);
            this.log.Information("Saved message {id}.", id);
        }

        public Maybe<string> Read(int id)
        {
            this.log.Debug("Reading message {id}.", id);
            var retVal = this.reader.Read(id);
            if (retVal.Any())
                this.log.Debug("Returning message {id}.", id);
            else
                this.log.Debug("No message {id} found.", id);
            return retVal;
        }
    }
}
