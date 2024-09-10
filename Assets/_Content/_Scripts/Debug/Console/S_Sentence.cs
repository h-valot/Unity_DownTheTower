public class Sentence
{
	private string _sentence;
	private string _colorCode;
	private Style _style;

	public Sentence(string sentence, Style style, string colorCode)
	{
		_sentence = sentence;
		_style = style;
		_colorCode = colorCode;
	}

	public string GetStylizedSentence()
	{
		string tagStart = "";
		string tagEnd = "";

		string colorTagStart = $"<#{_colorCode}>";
		string colorTagEnd = "</color>";

		switch (_style)
		{
			case Style.BOLD:
				tagStart = "<b>";
				tagEnd = "</b>";
				break;

			case Style.ITALIC:
				tagStart = "<i>";
				tagEnd = "</i>";
				break;
		}

		return colorTagStart + tagStart + _sentence + tagEnd + colorTagEnd;
	}
}