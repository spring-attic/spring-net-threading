using System.Collections;

namespace Spring.Threading.Collections
{
	/// <summary> 
	/// A <see cref="System.Collections.IDictionary"/> providing additional atomic
	/// <see cref="Spring.Threading.Collections.IConcurrentDictionary.PutIfAbsent(object, object)"/>,
	/// <see cref="Spring.Threading.Collections.IConcurrentDictionary.Remove(object, object)"/>,
	/// <see cref="Spring.Threading.Collections.IConcurrentDictionary.Replace(object, object)"/> methods.
	/// </summary>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	public interface IConcurrentDictionary : IDictionary
	{
		/// <summary> 
		/// If the specified key is not already associated
		/// with a value, associate it with the given value.
		/// </summary>
		/// <remarks>
		/// This is equivalent to:
		/// <code>
		///		if (!dictionary.Contains(key))
		/// 		return dictionary.Add(key, keyValue);
		/// 	else
		/// 		return dictionary[key[;
		/// </code>
		/// except that the action is performed atomically.
		/// </remarks>
		/// <param name="key">key with which the specified value is to be associated</param>
		/// <param name="keyValue">value to be associated with the specified key</param>
		/// <returns> 
		/// the previous value associated with the specified key, or
		/// <see lang="null"/> if there was no mapping for the key.
		/// (A <see lang="null"/> return can also indicate that the dictionary
		/// previously associated <see lang="null"/>  with the key,
		/// if the implementation supports null values.)
		/// </returns>
		object PutIfAbsent(object key, object keyValue);

		/// <summary> 
		/// Removes the entry for a key only if currently mapped to a given value.
		/// </summary>
		/// <remarks>
		/// This is equivalent to
		/// <code>
		///		if (dictionary.Contains(key) &amp;&amp; dictionary[key].Equals(keyValue)) {
		///			dictionary.Remove(key);
		///			return true;
		///		} else {
		///			return false;
		///		}
		///	</code>
		/// except that the action is performed atomically.
		/// </remarks>
		/// <param name="key">key with which the specified value is associated
		/// </param>
		/// <param name="keyValue">value expected to be associated with the specified key
		/// </param>
		/// <returns> <tt>true</tt> if the value was removed
		/// </returns>
		bool Remove(object key, object keyValue);

		/// <summary> 
		/// Replaces the entry for a key only if currently mapped to a given value.
		/// </summary>
		/// <remarks>
		/// This is equivalent to
		/// <code>
		///		if (dictionary.Contains(key) &amp;&amp; dictionary[key].Equals(oldValue)) {
		///			dictionary.Add(key, newValue);
		/// 		return true;
		/// 	} else {
		/// 		return false;
		/// 	}
		/// </code>
		/// except that the action is performed atomically.
		/// </remarks>
		/// <param name="key">key with which the specified value is associated
		/// </param>
		/// <param name="oldValue">value expected to be associated with the specified key
		/// </param>
		/// <param name="newValue">value to be associated with the specified key
		/// </param>
		/// <returns> <see lang="true"/>if the value was replaced</returns>
		bool Replace(object key, object oldValue, object newValue);

		/// <summary> 
		/// Replaces the entry for a key only if currently mapped to some value.
		/// </summary>
		/// <remarks>
		/// This is equivalent to
		/// <code>
		///		if (dictionary.Contains(key)) {
		/// 		return dictionar.Add(key, keyValue);
		/// 	} else { 
		/// 		return null;
		/// 	}
		/// </code>
		/// except that the action is performed atomically.
		/// </remarks>
		/// <param name="key">key with which the specified value is associated
		/// </param>
		/// <param name="keyValue">value to be associated with the specified key
		/// </param>
		/// <returns> 
		/// the previous value associated with the specified key, or
		/// <see lang="null"/> if there was no mapping for the key.
		/// (A <see lang="null"/> return can also indicate that the dictionary
		/// previously associated <see lang="null"/> with the key,
		/// if the implementation supports null values.)
		/// </returns>
		object Replace(object key, object keyValue);
	}
}