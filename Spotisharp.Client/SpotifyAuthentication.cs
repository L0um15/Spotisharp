using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace Spotisharp.Client;

public static class SpotifyAuthentication
{
    public static async Task<SpotifyClient?> CreateSpotifyClient()
    {
        string clientId = "b1d70eb4c56440f5b56537e96e079c7d";
        var (verifier, challenge) = PKCEUtil.GenerateCodes(120);

        Uri serverUri = new Uri("http://localhost:5000/auth");
        var loginRequest = new LoginRequest(serverUri, clientId, LoginRequest.ResponseType.Code)
        {
            CodeChallenge = challenge,
            CodeChallengeMethod = "S256"
        };
        
        BrowserUtil.Open(loginRequest.ToUri());

        var getCallbackTask = GetCallbackFromServer(serverUri, clientId);

        if (getCallbackTask.Wait(20000))
        {
            string authorizationCode = await getCallbackTask;
            var newResponse = await new OAuthClient().RequestToken
                (
                    new PKCETokenRequest
                        (
                            clientId,
                            authorizationCode,
                            serverUri,
                            verifier
                        )
                );
            return new SpotifyClient(newResponse.AccessToken);
        }
        else
        {
            return null;
        }
    }
    private static async Task<string> GetCallbackFromServer(Uri redirectUri, string clientID)
    {
        var embededAuthServer = new EmbedIOAuthServer(redirectUri, 5000);
        await embededAuthServer.Start();
        string authorizationCode = string.Empty;
        embededAuthServer.AuthorizationCodeReceived += async (sender, response) =>
        {
            await embededAuthServer.Stop();
            authorizationCode = response.Code;
        };

        while (authorizationCode == string.Empty)
        {
            await Task.Delay(1000);
        }

        return authorizationCode;
    }
}
