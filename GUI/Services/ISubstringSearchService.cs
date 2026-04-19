using GUI.Models;

namespace GUI.Services;

public interface ISubstringSearchService
{
	List<SubstringMatch> FindSubstrings(string input, SubstringTemplate template);
	List<SubstringMatch> FindNumberSubstrings(string input);
}
