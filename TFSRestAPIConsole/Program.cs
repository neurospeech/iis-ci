using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFSRestAPI;

namespace TFSRestAPIConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            TFS2012Client client = new TFS2012Client("", "", "", "") ;


            Task<IEnumerable<TFSCollection>> r = client.GetCollections();
            r.Wait();

            var rs = r.Result;

            Console.ReadLine();

        }
    }
}
