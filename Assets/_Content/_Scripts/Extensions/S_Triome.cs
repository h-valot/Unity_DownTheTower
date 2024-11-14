using System;

public enum TriomeEnum
{
	FALSE = 0,
	TRUE = 1,
	NONE = 2,
}

[Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
public struct Triome : IComparable<Triome>, IEquatable<Triome>
{
	public TriomeEnum m_Value;
	private static readonly string m_FalseString = "False";
	private static readonly string m_TrueString = "True";
	private static readonly string m_NoneString = "None";

	internal const int FALSE = 0;
	internal const int TRUE = 1;
	internal const int NONE = 2;

	public TriomeEnum value { get { return m_Value; } }

	public Triome(TriomeEnum value)
	{
		m_Value = value;
	}

	public Triome(int value)
	{
		m_Value = (TriomeEnum)value;
	}

	public static implicit operator Triome(TriomeEnum value)
	{
		return new Triome(value);
	}

	public static implicit operator Triome(int value)
	{
		return new Triome(value);
	}

	public static implicit operator TriomeEnum(Triome triome)
	{
		return triome.value;
	}


	public override int GetHashCode()
	{
		return m_Value.GetHashCode();
	}

	public override string ToString()
	{
		if (TriomeEnum.FALSE == m_Value)
		{
			return m_FalseString;
		}
		else if (TriomeEnum.TRUE == m_Value)
		{
			return m_TrueString;
		}
		return m_NoneString;
	}

	public string ToString(IFormatProvider provider)
	{
		return ToString();
	}

	public override bool Equals(object obj)
	{       
		// Assert: if it's not a boolean, we're definitely not equal
		if (!(obj is Triome)) return false;

		return m_Value == ((Triome)obj).m_Value;
	}

	public bool Equals(Triome other)
	{
		return m_Value == other.m_Value;
	}

	public int CompareTo(Triome other)
	{
		return m_Value.CompareTo(other.value);
	}

	public bool ToBool()
	{
		return m_Value == TriomeEnum.TRUE;
	}

	public static bool operator ==(Triome a, Triome b)
	{
		return a.CompareTo(b) == 0;
	}

	public static bool operator !=(Triome a, Triome b)
	{
		return !(a == b);
	}

	public static bool operator ==(Triome a, int b)
	{
		return ((int)a.m_Value).CompareTo(b) == 0;
	}

	public static bool operator !=(Triome a, int b)
	{
		return !(a == b);
	}
}