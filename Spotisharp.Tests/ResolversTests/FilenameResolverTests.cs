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
        [InlineData("\"sad\"", "sad")]
        [InlineData("[|DOMINO|]", "[DOMINO]")]
        [InlineData("*life is a diamond*", "life is a diamond")]
        [InlineData("Would you rather?", "Would you rather")]
        [InlineData("lost cause// (feat. Jesse)", "lost cause (feat. Jesse)")]
        [InlineData("Nightcore & KYANU Edit", "Nightcore KYANU Edit")]
        public void RemoveForbiddenChars_SpecialCharactersShouldBeRemoved(string filename, string expected)
        {
            var cleansed_filename = FilenameResolver.RemoveForbiddenChars(filename);
            Assert.Equal(expected, cleansed_filename);
        }   
    }
}
