using System;

namespace Spring.Collections
{
    [Flags] public enum CollectionOptions
    {
        Unique = 0x01,
        ReadOnly = 0x02,
        Fifo = 0x04,
        Bounded = 0x08,
        Unbounded = 0x10,
        NoNull = 0x20,
        Fair = 0x40,
        NoFair = 0x80,
    }
}