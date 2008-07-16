using System;

namespace Spring.Threading.Future
{
	/// <summary> 
	/// A mix-in style interface for marking objects that should be
	/// acted upon after a given delay.
	/// </summary>
	/// <remarks> 
	/// <p/>
	/// An implementation of this interface must define a
	/// <see cref="System.IComparable.CompareTo(object)"/> method that provides an ordering consistent with
	/// its <see cref="Spring.Threading.Future.IDelayed.GetRemainingDelay()"/> method.
	/// </remarks>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <changes>
	/// <ol>
	/// <li>Changed GetDelay return type from long to TimeSpan, and remove parameter.</li>
	/// </ol>
	/// </changes>
	public interface IDelayed : IComparable
	{
		/// <summary> 
		/// Returns the remaining delay associated with this object
		/// </summary>
		/// <returns>the remaining delay; zero or negative values indicate
		/// that the delay has already elapsed
		/// </returns>
		TimeSpan GetRemainingDelay();
	}
}