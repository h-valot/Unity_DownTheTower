public class Sentence
{
	private string m_sentence;
	private string m_colorCode;

	public Sentence(string sentence, string colorCode)
	{
		m_sentence = sentence;
		m_colorCode = colorCode;
	}

	public string GetStylizedSentence()
	{
		return $"<#{m_colorCode}>{m_sentence}</color>";
	}
}