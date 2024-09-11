public class Sentence
{
	private string _sentence;
	private string _colorCode;

	public Sentence(string sentence, string colorCode)
	{
		_sentence = sentence;
		_colorCode = colorCode;
	}

	public string GetStylizedSentence()
	{
		return $"<#{_colorCode}>{_sentence}</color>";
	}
}