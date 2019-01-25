using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ploeh.Samples.Encapsulation.CodeExamples
{
    public class SqlStore : IStoreReader, IStoreWriter
    {
        public void Save(int id, string message)
        {
            // Write to database here
        }

        public Maybe<string> Read(int id)
        {
            // Read from database here
            return new Maybe<string>();
        }
    }
}
