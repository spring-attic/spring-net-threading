#region License

/*
 * Copyright © 2002-2006 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System.Collections;

namespace Spring.Collections
{
	/// <summary>
	/// An interface representing a <see cref="System.Collections.IDictionary"/>, sorted in ascending order.
	/// </summary>
	/// <author>Griffin Caprio</author>
	public interface ISortedDictionary : IDictionary
	{
		/// <summary>
		/// Gets a flag indicating if the <see cref="ISortedDictionary"/> is empty.
		/// </summary>
		bool IsEmpty { get; }
		/// <summary>
		/// Gets the first (lowest) key in this dictionary
		/// </summary>
		object FirstKey { get; }
		/// <summary>
		/// Gets the last ( highest ) key in this dictionary
		/// </summary>
		object LastKey { get; }
		/// <summary>
		/// Returns a <see cref="Spring.Collections.ISortedDictionary"/> view of this dictionary, where all elements have keys less
		/// than <paramref name="ceilingKey"/>
		/// </summary>
		/// <param name="ceilingKey">the key</param>
		/// <returns>A <see cref="Spring.Collections.ISortedDictionary"/> view of this dictionary, where all elements have keys less
		/// than <paramref name="ceilingKey"/></returns>
		ISortedDictionary HeadDictionary( object ceilingKey );
		/// <summary>
		/// Returns a <see cref="Spring.Collections.ISortedDictionary"/> view of this dictionary, where all elements have keys greater than
		/// <paramref name="floorKey"/> inclusive and less than <paramref name="ceilingKey"/> exclusive.
		/// </summary>
		/// <param name="floorKey">lowest key</param>
		/// <param name="ceilingKey">highest key</param>
		/// <returns>A <see cref="Spring.Collections.ISortedDictionary"/> view of this dictionary, where all elements have keys greater than
		/// <paramref name="floorKey"/> inclusive and less than <paramref name="ceilingKey"/> exclusive.</returns>
		ISortedDictionary SubDictionary( object floorKey, object ceilingKey );
		/// <summary>
		/// Returns a <see cref="Spring.Collections.ISortedDictionary"/>  view of this dictionary, where all elements have keys greater than
		/// <paramref name="floorKey"/>
		/// </summary>
		/// <param name="floorKey">the key</param>
		/// <returns>A <see cref="Spring.Collections.ISortedDictionary"/>  view of this dictionary, where all elements have keys greater than
		/// <paramref name="floorKey"/></returns>
		ISortedDictionary TailDictionary( object floorKey );
	}
}
