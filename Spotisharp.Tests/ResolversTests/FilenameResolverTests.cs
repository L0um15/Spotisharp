using Spotisharp.Client.Enums;
using Spotisharp.Client.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Spotisharp.Tests.ResolversTests
{
    public class FilenameResolverTests
    {
        [Theory]
        [InlineData("\"sad\"", "sad", StringType.Filename)]
        [InlineData("[|DOMINO|]", "[DOMINO]", StringType.Filename)]
        [InlineData("*life is a diamond*", "life is a diamond", StringType.Filename)]
        [InlineData("Would you rather?", "Would you rather", StringType.Filename)]
        [InlineData("lost cause// (feat. Jesse)", "lost cause (feat. Jesse)", StringType.Filename)]
        [InlineData("Nightcore & KYANU Edit", "Nightcore & KYANU Edit", StringType.Filename)]

        [InlineData("TEST&+,;@$%#!=<>:\"/\\|?* PASSED", "TEST PASSED",StringType.Url)]
        public void RemoveForbiddenChars_SpecialCharactersShouldBeRemoved(string filename, string expected, StringType sType)
        {
            var cleansed_filename = FilenameResolver.RemoveForbiddenChars(filename, sType);
            Assert.Equal(expected, cleansed_filename);
        }   
    }
}
