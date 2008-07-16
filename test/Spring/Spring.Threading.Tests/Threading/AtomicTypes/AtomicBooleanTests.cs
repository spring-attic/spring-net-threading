using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NUnit.Framework;

namespace Spring.Threading.AtomicTypes
{
	[TestFixture]
	public class AtomicBooleanTest : BaseThreadingTestCase
	{
		private class AnonymousClassRunnable
		{
			private AtomicBoolean _atomicBoolean;

			public AnonymousClassRunnable( AtomicBoolean ai )
			{
				_atomicBoolean = ai;
			}

			public void Run()
			{
				while ( !_atomicBoolean.CompareAndSet( false, true ) )
				{
					Thread.Sleep( SHORT_DELAY_MS );
				}
			}
		}

		[Test]
		public void Constructor()
		{
			AtomicBoolean ai = new AtomicBoolean( true );
			Assert.AreEqual( true, ai.BooleanValue );
		}

		[Test]
		public void DefaultConstructor()
		{
			AtomicBoolean ai = new AtomicBoolean();
			Assert.AreEqual( false, ai.BooleanValue );
		}

		[Test]
		public void GetLastSetValue()
		{
			AtomicBoolean ai = new AtomicBoolean( true );
			Assert.AreEqual( true, ai.BooleanValue );
			ai.BooleanValue = false;
			Assert.AreEqual( false, ai.BooleanValue );
			ai.BooleanValue = true;
			Assert.AreEqual( true, ai.BooleanValue );
		}

		[Test]
		public void GetLastLazySetValue()
		{
			AtomicBoolean ai = new AtomicBoolean( true );
			Assert.AreEqual( true, ai.BooleanValue );
			ai.LazySet( false );
			Assert.AreEqual( false, ai.BooleanValue );
			ai.LazySet( true );
			Assert.AreEqual( true, ai.BooleanValue );
		}

		[Test]
		public void CompareExpectedValueAndSetNewValue()
		{
			AtomicBoolean ai = new AtomicBoolean( true );
			Assert.AreEqual( true, ai.BooleanValue );
			Assert.IsTrue( ai.CompareAndSet( true, false ) );
			Assert.AreEqual( false, ai.BooleanValue );
			Assert.IsTrue( ai.CompareAndSet( false, false ) );
			Assert.AreEqual( false, ai.BooleanValue );
			Assert.IsFalse( ai.CompareAndSet( true, false ) );
			Assert.IsFalse( ( ai.BooleanValue ) );
			Assert.IsTrue( ai.CompareAndSet( false, true ) );
			Assert.AreEqual( true, ai.BooleanValue );
		}

		[Test]
		public void CompareExpectedValueAndSetNewValueInMultipleThreads()
		{
			AtomicBoolean ai = new AtomicBoolean( true );
			Thread t = new Thread( new ThreadStart( new AnonymousClassRunnable( ai ).Run ) );

			t.Start();
			Assert.IsTrue( ai.CompareAndSet( true, false ), "Value" );
			t.Join( SMALL_DELAY_MS );
			Assert.IsFalse( t.IsAlive, "Thread is still alive." );
			Assert.IsTrue( ai.BooleanValue );
		}

		[Test]
		public void WeakCompareExpectedValueAndSetNewValue()
		{
			AtomicBoolean ai = new AtomicBoolean( true );
			while ( !ai.WeakCompareAndSet( true, false ) )
			{
				;
			}
			Assert.AreEqual( false, ai.BooleanValue );
			while ( !ai.WeakCompareAndSet( false, false ) )
			{
				;
			}
			Assert.AreEqual( false, ai.BooleanValue );
			while ( !ai.WeakCompareAndSet( false, true ) )
			{
				;
			}
			Assert.AreEqual( true, ai.BooleanValue );
			Assert.IsFalse( ai.WeakCompareAndSet( false, true ) );
		}

		[Test]
		public void GetOldValueAndSetNewValue()
		{
			AtomicBoolean ai = new AtomicBoolean( true );
			Assert.AreEqual( true, ai.SetNewAtomicValue( false ) );
			Assert.AreEqual( false, ai.SetNewAtomicValue( false ) );
			Assert.AreEqual( false, ai.SetNewAtomicValue( true ) );
			Assert.AreEqual( true, ai.BooleanValue );
		}

		[Test]
		public void SerializationOfAtomicBooleanValue()
		{
			AtomicBoolean atomicBoolean = new AtomicBoolean();

			atomicBoolean.BooleanValue = true;
			MemoryStream bout = new MemoryStream( 10000 );

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize( bout, atomicBoolean );

			MemoryStream bin = new MemoryStream( bout.ToArray() );
			BinaryFormatter formatter2 = new BinaryFormatter();
			AtomicBoolean r = (AtomicBoolean) formatter2.Deserialize( bin );
			Assert.AreEqual( atomicBoolean.BooleanValue, r.BooleanValue );
		}

		[Test]
		public void ToStringRepresentation()
		{
			AtomicBoolean ai = new AtomicBoolean();
			Assert.AreEqual( ai.ToString(), Boolean.FalseString );
			ai.BooleanValue = true;
			Assert.AreEqual( ai.ToString(), Boolean.TrueString );
		}
	}
}