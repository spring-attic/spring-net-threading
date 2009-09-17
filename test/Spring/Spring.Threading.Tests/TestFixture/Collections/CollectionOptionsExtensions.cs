using NUnit.Framework;

namespace Spring.TestFixture.Collections
{
    public static class CollectionOptionsExtensions
    {
        private const string SkipMessage = "Skipped. This test case doesn't apply to test subject {0} of options: {1}";

        public static bool Has(this CollectionOptions options, CollectionOptions flags)
        {
            return (options & flags) != 0;
        }

        public static bool HasAll(this CollectionOptions options, CollectionOptions flags)
        {
            return (options & flags) == flags;
        }

        public static bool Misses(this CollectionOptions options, CollectionOptions flags)
        {
            return (options & flags) != flags;
        }

        public static bool MissesAll(this CollectionOptions options, CollectionOptions flags)
        {
            return (options & flags) == 0;
        }

        public static void SkipWhen(this CollectionOptions options, CollectionOptions flags)
        {
            if (options.Has(flags))
            {
                Assert.Pass(SkipMessage, "with any", flags);
            }
        }

        public static void SkipWhenAll(this CollectionOptions options, CollectionOptions flags)
        {
            if (options.HasAll(flags))
            {
                Assert.Pass(SkipMessage, "with all", flags);
            }
        }

        public static void SkipWhenNot(this CollectionOptions options, CollectionOptions flags)
        {
            if (options.Misses(flags))
            {
                Assert.Pass(SkipMessage, "missing any",  flags);
            }
        }

        public static void SkipWhenNotAll(this CollectionOptions options, CollectionOptions flags)
        {
            if (options.MissesAll(flags))
            {
                Assert.Pass(SkipMessage, "missing all", flags);
            }
        }

        public static CollectionOptions Set(this CollectionOptions options, CollectionOptions flags, bool value)
        {
            return value ? options.Set(flags) : options.Unset(flags);
        }

        public static CollectionOptions Set(this CollectionOptions options, CollectionOptions flags)
        {
            return options | flags;
        }

        public static CollectionOptions Unset(this CollectionOptions options, CollectionOptions flags)
        {
            return options & ~flags;
        }
    }
}