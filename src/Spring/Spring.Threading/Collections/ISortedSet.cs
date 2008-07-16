using System;

namespace Spring.Collections
{
	/// <summary>
	/// An interface representing a <see cref="ISet"/>, sorted in ascending order.
	/// </summary>
	/// <author>Griffin Caprio</author>
	public interface ISortedSet : ISet
	{
		/// <summary>
		/// Returns a portion of the list whose elements are less than the limit object parameter.
		/// </summary>
		/// <param name="limit">The end element of the portion to extract.</param>
		/// <returns>The portion of the collection whose elements are less than the limit object parameter.</returns>
		ISortedSet HeadSet( Object limit );

		/// <summary>
		/// Returns a portion of the list whose elements are greater that the lowerLimit parameter less than the upperLimit parameter.
		/// </summary>
		/// <param name="lowerLimit">The start element of the portion to extract.</param>
		/// <param name="upperLimit">The end element of the portion to extract.</param>
		/// <returns>The portion of the collection.</returns>
		ISortedSet SubSet( Object lowerLimit, Object upperLimit );

		/// <summary>
		/// Returns a portion of the list whose elements are greater than the limit object parameter.
		/// </summary>
		/// <param name="limit">The start element of the portion to extract.</param>
		/// <returns>The portion of the collection whose elements are greater than the limit object parameter.</returns>
		ISortedSet TailSet( Object limit );
	}
}