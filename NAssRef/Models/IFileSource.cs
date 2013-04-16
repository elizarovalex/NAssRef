namespace AssRef.Models
{
	public interface IFileSource
	{
		AssRefItem[] GetAssRefList(string directoryPath, bool isUseSubdir);
	}
}