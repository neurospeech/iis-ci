using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISCI
{
    public class GlobalLock : IDisposable
    {

        private static Dictionary<string, string> locks = new Dictionary<string, string>();

        public static IEnumerable<string> GetActiveLocks() {
            lock (locks) {
                return locks.Keys.ToList();
            }
        }


        private bool locked = false;
        private string lockName;

        public GlobalLock(string lockName)
        {
            this.lockName = lockName;
        }

        public bool AcquireLock() {
            lock (locks)
            {
                if (!locks.ContainsKey(lockName))
                {
                    locked = true;
                    locks[lockName] = lockName;
                }
            }
            return locked;
        }


        public void Dispose()
        {
            if (locked) {
                lock (locks) {
                    locks.Remove(lockName);
                }
            }
        }
    }
}
