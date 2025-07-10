using System.Collections.Concurrent;

namespace ChatBlaster.Infrastructure
{
    public static class PortAllocator
    {
        private static readonly ConcurrentDictionary<int, bool> _allocatedPorts = new();
        private static readonly object _portLock = new object();
        private static readonly int _minPort = 9222;
        private static readonly int _maxPort = 10000;
        private static int _lastPort = _minPort - 1;
        private static readonly ConcurrentDictionary<int, byte> _inUse = new();
        public static int Next()
        {
            while (true)
            {
                lock (_portLock)
                {
                    // Find the next available port, looping within [_minPort, _maxPort]
                    int start = (_lastPort < _minPort ? _minPort : _lastPort + 1);
                    if (start > _maxPort) start = _minPort;
                    int candidate = start;
                    // Loop until we find a free port that is not already allocated
                    while (_allocatedPorts.ContainsKey(candidate))
                    {
                        candidate++;
                        if (candidate > _maxPort) candidate = _minPort;
                        if (candidate == start)
                        {
                            // No free port found (all ports are in use)
                            throw new InvalidOperationException("No available ports");
                        }
                    }
                    // Reserve the found port and update _lastPort
                    _allocatedPorts[candidate] = true;
                    _lastPort = candidate;
                    return candidate;
                }
            }
        }
        public static void Release(int port)
        {
            lock (_portLock)
            {
                _allocatedPorts.TryRemove(port, out _);
            }
        }
    }
}
