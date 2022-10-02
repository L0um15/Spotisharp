using Spotisharp.Client.Enums;
using System.Text;

namespace Spotisharp.Client.Resolvers;

public static class FilenameResolver
{

    private static char[] _forbiddenFilenameChars = { '<', '>', ':', '\"', '/', '\\', '|', '?', '*' };
    private static char[] _forbiddenUrlChars = { '&', '+', ',', ';', '@', '?', '$', '%', '#', '!', '=', ':' };


    public static string RemoveForbiddenChars(string input, StringType sType)
    {
        ReadOnlySpan<char> charSpan;
        ReadOnlySpan<char> inputSpan = input.AsSpan();
        if (sType == StringType.Filename)
        {
            charSpan = _forbiddenFilenameChars.AsSpan();
        }
        else if(sType == StringType.Url)
        {
            charSpan = _forbiddenUrlChars.AsSpan();
        }
        else
        {
            return input; 
        }
        StringBuilder sb = new StringBuilder();

        bool matchFound = false;

        for(int i = 0; i < inputSpan.Length; i++)
        {
            matchFound = false;
            for(int j = 0; j < charSpan.Length; j++)
            {
                if (inputSpan[i] == charSpan[j])
                {
                    matchFound = true;
                }
            }
            if (!matchFound)
            {
                sb.Append(inputSpan[i]);
            }
            else
            {
                if(sType == StringType.Url)
                {
                    sb.Append(' ');
                }
            }
        }
        return sb.ToString();
    }
}
