using Spring.Collections;

namespace Spring.Threading.Collections
{
	/// <summary> A <see cref="Spring.Threading.Collections.IConcurrentDictionary"/> supporting <see cref="Spring.Collections.INavigableDictionary"/>
	/// operations,and recursively so for its navigable sub-maps.
	/// </summary>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	public interface IConcurrentNavigableDictionary : IConcurrentDictionary, INavigableDictionary
	{
	}
}