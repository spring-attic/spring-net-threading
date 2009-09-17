using System;

namespace Spring.TestFixture.Collections
{
    [Flags] public enum CollectionOptions
    {
        Unique = 0x01,
        ReadOnly = 0x02,
        Fifo = 0x04,
        Unbounded = 0x08,
        NoNull = 0x10,
        Fair = 0x20,
        ToStringPrintItems = 0x80,
    }
}