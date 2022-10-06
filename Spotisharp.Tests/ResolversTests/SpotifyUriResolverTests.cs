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
    public class SpotifyUriResolverTests
    {
        #region IsUrlValid_Theory_ReturnsTrue
        [Theory]
        [InlineData("spotify:track:3wXw9CsZsxca37dbzHUx4g")]
        [InlineData("http://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4g")]
        [InlineData("https://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4g")]
        [InlineData("http://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4g?si=14a6f2cdee324d66")]
        [InlineData("https://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4g?si=14a6f2cdee324d66")]

        [InlineData("spotify:playlist:1PZIEAy8yEcD3Z41wAJ74i")]
        [InlineData("http://open.spotify.com/playlist/1PZIEAy8yEcD3Z41wAJ74i")]
        [InlineData("https://open.spotify.com/playlist/1PZIEAy8yEcD3Z41wAJ74i")]
        [InlineData("http://open.spotify.com/playlist/1PZIEAy8yEcD3Z41wAJ74i?si=ccdec83eee36444b")]
        [InlineData("https://open.spotify.com/playlist/1PZIEAy8yEcD3Z41wAJ74i?si=ccdec83eee36444b")]

        [InlineData("spotify:album:3PJhr2ejWvavjQGlBJvEkn")]
        [InlineData("http://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEkn")]
        [InlineData("https://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEkn")]
        [InlineData("http://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEkn?si=Eztk-wmJRJWFvHCdn__fqQ")]
        [InlineData("https://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEkn?si=Eztk-wmJRJWFvHCdn__fqQ")]

        #endregion
        public void IsUrlValid_ReturnsTrue(string url)
        {
            bool result = SpotifyUriResolver.IsUriValid(url);
            Assert.True(result);
        }

        #region IsUrlValid_Theory_ReturnsFalse
        [Theory]
        [InlineData("spotify:track:3wXw9CsZsxca37dbzHUx4")]                                         // Id is too short
        [InlineData("spotify:track:3wXw9CsZsxca37dbzHUx4_")]                                        // Illegal char is present
        [InlineData("spotify:tracks:3wXw9CsZsxca37dbzHUx4")]                                        // Invalid type

        [InlineData("spotify:playlist:1PZIEAy8yEcD3Z41wAJ74")]                                      // Id is too short
        [InlineData("spotify:playlist:1PZIEAy8yEcD3Z41wAJ74_")]                                     // Illegal char is present
        [InlineData("spotify:playlists:1PZIEAy8yEcD3Z41wAJ74i")]                                    // Invalid type

        [InlineData("spotify:album:4jyZr8pSN694iMSuFeyEV")]                                         // Id is too short
        [InlineData("spotify:album:4jyZr8pSN694iMSuFeyEV_")]                                        // Illegal char is present
        [InlineData("spotify:albums:4jyZr8pSN694iMSuFeyEVV")]                                       // Invalid type


        [InlineData("http://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4")]                         // Id is too short
        [InlineData("http://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4_")]                        // Illegal char is present
        [InlineData("http://open.spotify.com/tracks/3wXw9CsZsxca37dbzHUx4g")]                       // Invalid type

        [InlineData("https://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4")]                        // Id is too short
        [InlineData("https://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4_")]                       // Illegal char is present
        [InlineData("https://open.spotify.com/tracks/3wXw9CsZsxca37dbzHUx4g")]                      // Invalid type

        [InlineData("http://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4?si=14a6f2cdee324d66")]     // Id is too short
        [InlineData("http://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4_?si=14a6f2cdee324d66")]    // Illegal char is present
        [InlineData("http://open.spotify.com/tracks/3wXw9CsZsxca37dbzHUx4g?si=14a6f2cdee324d66")]   // Invalid type

        [InlineData("https://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4?si=14a6f2cdee324d66")]   // Id is too short
        [InlineData("https://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4_?si=14a6f2cdee324d66")]   // Illegal char is present
        [InlineData("https://open.spotify.com/tracks/3wXw9CsZsxca37dbzHUx4g?si=14a6f2cdee324d66")]  // Invalid type

        [InlineData("http://open.spotify.com/playlist/3wXw9CsZsxca37dbzHUx4")]                         // Id is too short
        [InlineData("http://open.spotify.com/playlist/3wXw9CsZsxca37dbzHUx4_")]                        // Illegal char is present
        [InlineData("http://open.spotify.com/playlists/3wXw9CsZsxca37dbzHUx4g")]                       // Invalid type

        [InlineData("https://open.spotify.com/playlist/3wXw9CsZsxca37dbzHUx4")]                        // Id is too short
        [InlineData("https://open.spotify.com/playlist/3wXw9CsZsxca37dbzHUx4_")]                       // Illegal char is present
        [InlineData("https://open.spotify.com/playlists/3wXw9CsZsxca37dbzHUx4g")]                      // Invalid type

        [InlineData("http://open.spotify.com/playlist/3wXw9CsZsxca37dbzHUx4?si=14a6f2cdee324d66")]     // Id is too short
        [InlineData("http://open.spotify.com/playlist/3wXw9CsZsxca37dbzHUx4_?si=14a6f2cdee324d66")]    // Illegal char is present
        [InlineData("http://open.spotify.com/playlists/3wXw9CsZsxca37dbzHUx4g?si=14a6f2cdee324d66")]   // Invalid type

        [InlineData("https://open.spotify.com/playlist/3wXw9CsZsxca37dbzHUx4?si=14a6f2cdee324d66")]    // Id is too short
        [InlineData("https://open.spotify.com/playlist/3wXw9CsZsxca37dbzHUx4_?si=14a6f2cdee324d66")]   // Illegal char is present
        [InlineData("https://open.spotify.com/playlists/3wXw9CsZsxca37dbzHUx4g?si=14a6f2cdee324d66")]  // Invalid type

        [InlineData("http://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEk")]                            // Id is too short
        [InlineData("http://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEk_")]                           // Illegal char is present
        [InlineData("http://open.spotify.com/albums/3PJhr2ejWvavjQGlBJvEkn")]                          // Invalid type

        [InlineData("https://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEk")]                           // Id is too short
        [InlineData("https://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEk_")]                          // Illegal char is present
        [InlineData("https://open.spotify.com/albums/3PJhr2ejWvavjQGlBJvEkn")]                         // Invalid type

        [InlineData("http://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEk?si=Eztk-wmJRJWFvHCdn__fqQ")]                           // Id is too short
        [InlineData("http://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEk_?si=Eztk-wmJRJWFvHCdn__fqQ")]                          // Illegal char is present
        [InlineData("http://open.spotify.com/albums/3PJhr2ejWvavjQGlBJvEkn?si=Eztk-wmJRJWFvHCdn__fqQ")]                         // Invalid type

        [InlineData("https://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEk?si=Eztk-wmJRJWFvHCdn__fqQ")]                          // Id is too short
        [InlineData("https://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEk_?si=Eztk-wmJRJWFvHCdn__fqQ")]                         // Illegal char is present
        [InlineData("https://open.spotify.com/albums/3PJhr2ejWvavjQGlBJvEkn?si=Eztk-wmJRJWFvHCdn__fqQ")]                        // Invalid type

        #endregion
        public void IsUrlValid_ReturnsFalse(string url)
        {
            bool result = SpotifyUriResolver.IsUriValid(url);
            Assert.False(result);
        }

        #region GetUriType_Theory_ReturnsTypeUrl
        [Theory]
        [InlineData("http://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4g")]
        [InlineData("https://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4g")]
        [InlineData("http://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4g?si=14a6f2cdee324d66")]
        [InlineData("https://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4g?si=14a6f2cdee324d66")]

        [InlineData("http://open.spotify.com/playlist/1PZIEAy8yEcD3Z41wAJ74i")]
        [InlineData("https://open.spotify.com/playlist/1PZIEAy8yEcD3Z41wAJ74i")]
        [InlineData("http://open.spotify.com/playlist/1PZIEAy8yEcD3Z41wAJ74i?si=ccdec83eee36444b")]
        [InlineData("https://open.spotify.com/playlist/1PZIEAy8yEcD3Z41wAJ74i?si=ccdec83eee36444b")]

        [InlineData("http://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEkn")]
        [InlineData("https://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEkn")]
        [InlineData("http://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEkn?si=Eztk-wmJRJWFvHCdn__fqQ")]
        [InlineData("https://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEkn?si=Eztk-wmJRJWFvHCdn__fqQ")]
        #endregion
        public void GetUriType_ReturnsTypeUrl(string uri)
        {
            SpotifyUriType result = SpotifyUriResolver.GetUriType(uri);
            Assert.Equal(SpotifyUriType.Url, result);
        }

        #region GetUriType_Theory_ReturnsTypeUrn
        [Theory]
        [InlineData("spotify:track:3wXw9CsZsxca37dbzHUx4g")]
        [InlineData("spotify:playlist:1PZIEAy8yEcD3Z41wAJ74i")]
        [InlineData("spotify:album:3PJhr2ejWvavjQGlBJvEkn")]
        #endregion
        public void GetUriType_ReturnsTypeUrn(string uri)
        {
            SpotifyUriType result = SpotifyUriResolver.GetUriType(uri);
            Assert.Equal(SpotifyUriType.Urn, result);
        }

        #region GetUriType_Theory_ReturnsTypeNone
        [Theory]
        [InlineData("spotify:track:3wXw9CsZsxca37dbzHUx4")]                                         // Id is too short
        [InlineData("spotify:track:3wXw9CsZsxca37dbzHUx4_")]                                        // Illegal char is present
        [InlineData("spotify:tracks:3wXw9CsZsxca37dbzHUx4")]                                        // Invalid type

        [InlineData("spotify:playlist:1PZIEAy8yEcD3Z41wAJ74")]                                      // Id is too short
        [InlineData("spotify:playlist:1PZIEAy8yEcD3Z41wAJ74_")]                                     // Illegal char is present
        [InlineData("spotify:playlists:1PZIEAy8yEcD3Z41wAJ74i")]                                    // Invalid type

        [InlineData("spotify:album:4jyZr8pSN694iMSuFeyEV")]                                         // Id is too short
        [InlineData("spotify:album:4jyZr8pSN694iMSuFeyEV_")]                                        // Illegal char is present
        [InlineData("spotify:albums:4jyZr8pSN694iMSuFeyEVV")]                                       // Invalid type


        [InlineData("http://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4")]                         // Id is too short
        [InlineData("http://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4_")]                        // Illegal char is present
        [InlineData("http://open.spotify.com/tracks/3wXw9CsZsxca37dbzHUx4g")]                       // Invalid type

        [InlineData("https://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4")]                        // Id is too short
        [InlineData("https://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4_")]                       // Illegal char is present
        [InlineData("https://open.spotify.com/tracks/3wXw9CsZsxca37dbzHUx4g")]                      // Invalid type

        [InlineData("http://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4?si=14a6f2cdee324d66")]     // Id is too short
        [InlineData("http://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4_?si=14a6f2cdee324d66")]    // Illegal char is present
        [InlineData("http://open.spotify.com/tracks/3wXw9CsZsxca37dbzHUx4g?si=14a6f2cdee324d66")]   // Invalid type

        [InlineData("https://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4?si=14a6f2cdee324d66")]   // Id is too short
        [InlineData("https://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4_?si=14a6f2cdee324d66")]   // Illegal char is present
        [InlineData("https://open.spotify.com/tracks/3wXw9CsZsxca37dbzHUx4g?si=14a6f2cdee324d66")]  // Invalid type

        [InlineData("http://open.spotify.com/playlist/3wXw9CsZsxca37dbzHUx4")]                         // Id is too short
        [InlineData("http://open.spotify.com/playlist/3wXw9CsZsxca37dbzHUx4_")]                        // Illegal char is present
        [InlineData("http://open.spotify.com/playlists/3wXw9CsZsxca37dbzHUx4g")]                       // Invalid type

        [InlineData("https://open.spotify.com/playlist/3wXw9CsZsxca37dbzHUx4")]                        // Id is too short
        [InlineData("https://open.spotify.com/playlist/3wXw9CsZsxca37dbzHUx4_")]                       // Illegal char is present
        [InlineData("https://open.spotify.com/playlists/3wXw9CsZsxca37dbzHUx4g")]                      // Invalid type

        [InlineData("http://open.spotify.com/playlist/3wXw9CsZsxca37dbzHUx4?si=14a6f2cdee324d66")]     // Id is too short
        [InlineData("http://open.spotify.com/playlist/3wXw9CsZsxca37dbzHUx4_?si=14a6f2cdee324d66")]    // Illegal char is present
        [InlineData("http://open.spotify.com/playlists/3wXw9CsZsxca37dbzHUx4g?si=14a6f2cdee324d66")]   // Invalid type

        [InlineData("https://open.spotify.com/playlist/3wXw9CsZsxca37dbzHUx4?si=14a6f2cdee324d66")]    // Id is too short
        [InlineData("https://open.spotify.com/playlist/3wXw9CsZsxca37dbzHUx4_?si=14a6f2cdee324d66")]   // Illegal char is present
        [InlineData("https://open.spotify.com/playlists/3wXw9CsZsxca37dbzHUx4g?si=14a6f2cdee324d66")]  // Invalid type

        [InlineData("http://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEk")]                            // Id is too short
        [InlineData("http://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEk_")]                           // Illegal char is present
        [InlineData("http://open.spotify.com/albums/3PJhr2ejWvavjQGlBJvEkn")]                          // Invalid type

        [InlineData("https://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEk")]                           // Id is too short
        [InlineData("https://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEk_")]                          // Illegal char is present
        [InlineData("https://open.spotify.com/albums/3PJhr2ejWvavjQGlBJvEkn")]                         // Invalid type

        [InlineData("http://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEk?si=Eztk-wmJRJWFvHCdn__fqQ")]                           // Id is too short
        [InlineData("http://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEk_?si=Eztk-wmJRJWFvHCdn__fqQ")]                          // Illegal char is present
        [InlineData("http://open.spotify.com/albums/3PJhr2ejWvavjQGlBJvEkn?si=Eztk-wmJRJWFvHCdn__fqQ")]                         // Invalid type

        [InlineData("https://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEk?si=Eztk-wmJRJWFvHCdn__fqQ")]                          // Id is too short
        [InlineData("https://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEk_?si=Eztk-wmJRJWFvHCdn__fqQ")]                         // Illegal char is present
        [InlineData("https://open.spotify.com/albums/3PJhr2ejWvavjQGlBJvEkn?si=Eztk-wmJRJWFvHCdn__fqQ")]                        // Invalid type
        #endregion
        public void GetUriType_ReturnsTypeNone(string uri)
        {
            SpotifyUriType result = SpotifyUriResolver.GetUriType(uri);
            Assert.Equal(SpotifyUriType.None, result);
        }

        #region GetID_Theory_ReturnsString
        [Theory]
        [InlineData("spotify:track:3wXw9CsZsxca37dbzHUx4g", "3wXw9CsZsxca37dbzHUx4g", SpotifyUriType.Urn)]
        [InlineData("http://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4g", "3wXw9CsZsxca37dbzHUx4g", SpotifyUriType.Url)]
        [InlineData("https://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4g", "3wXw9CsZsxca37dbzHUx4g", SpotifyUriType.Url)]
        [InlineData("http://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4g?si=14a6f2cdee324d66", "3wXw9CsZsxca37dbzHUx4g", SpotifyUriType.Url)]
        [InlineData("https://open.spotify.com/track/3wXw9CsZsxca37dbzHUx4g?si=14a6f2cdee324d66", "3wXw9CsZsxca37dbzHUx4g", SpotifyUriType.Url)]

        [InlineData("spotify:playlist:1PZIEAy8yEcD3Z41wAJ74i", "1PZIEAy8yEcD3Z41wAJ74i", SpotifyUriType.Urn)]
        [InlineData("http://open.spotify.com/playlist/1PZIEAy8yEcD3Z41wAJ74i", "1PZIEAy8yEcD3Z41wAJ74i", SpotifyUriType.Url)]
        [InlineData("https://open.spotify.com/playlist/1PZIEAy8yEcD3Z41wAJ74i", "1PZIEAy8yEcD3Z41wAJ74i", SpotifyUriType.Url)]
        [InlineData("http://open.spotify.com/playlist/1PZIEAy8yEcD3Z41wAJ74i?si=ccdec83eee36444b", "1PZIEAy8yEcD3Z41wAJ74i", SpotifyUriType.Url)]
        [InlineData("https://open.spotify.com/playlist/1PZIEAy8yEcD3Z41wAJ74i?si=ccdec83eee36444b", "1PZIEAy8yEcD3Z41wAJ74i", SpotifyUriType.Url)]

        [InlineData("spotify:album:3PJhr2ejWvavjQGlBJvEkn", "3PJhr2ejWvavjQGlBJvEkn", SpotifyUriType.Urn)]
        [InlineData("http://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEkn", "3PJhr2ejWvavjQGlBJvEkn", SpotifyUriType.Url)]
        [InlineData("https://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEkn", "3PJhr2ejWvavjQGlBJvEkn", SpotifyUriType.Url)]
        [InlineData("http://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEkn?si=Eztk-wmJRJWFvHCdn__fqQ", "3PJhr2ejWvavjQGlBJvEkn", SpotifyUriType.Url)]
        [InlineData("https://open.spotify.com/album/3PJhr2ejWvavjQGlBJvEkn?si=Eztk-wmJRJWFvHCdn__fqQ", "3PJhr2ejWvavjQGlBJvEkn", SpotifyUriType.Url)]

        #endregion
        public void GetID_ExtractIDFromUri_ReturnsString(string uri, string expected,SpotifyUriType sType)
        {
            var actual = SpotifyUriResolver.GetID(uri, sType);
            Assert.Equal(expected, actual);
        }
    }
}
