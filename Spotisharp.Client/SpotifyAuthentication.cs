﻿using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using Spotisharp.Client.Models;
using System.Text.Json;

public static class SpotifyAuthentication
{

    private static PKCETokenModel _pkceTokenModel = new PKCETokenModel();
    
    private static string _tokenConfigDir =
        Path.Join
        (
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "spotisharp"
        );
    private static string _tokenConfigFile = Path.Join(_tokenConfigDir, "auth.json");

    public static async Task<SpotifyClient?> CreateSpotifyClient()
    {
        string clientId = "b1d70eb4c56440f5b56537e96e079c7d";

        if (File.Exists(_tokenConfigFile))
        {
            string tokenConfigContent = File.ReadAllText(_tokenConfigFile);
            var deserializedJson = JsonSerializer.Deserialize<PKCETokenModel>(tokenConfigContent);
            if(deserializedJson != null)
            {
                var newResponse = await TryGetPKCERefreshTokenResponse(clientId,deserializedJson.RefreshToken);
                if(newResponse != null)
                {
                    deserializedJson.RefreshToken = newResponse.RefreshToken;
                    string serializedJson = JsonSerializer.Serialize(deserializedJson);
                    File.WriteAllText(_tokenConfigFile, serializedJson);
                    return new SpotifyClient(newResponse.AccessToken);
                }
            }
        }

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

            _pkceTokenModel.RefreshToken = newResponse.RefreshToken;
            string serializedJson = JsonSerializer.Serialize(_pkceTokenModel);
            File.WriteAllText(_tokenConfigFile, serializedJson);

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

    private static async Task<PKCETokenResponse?> TryGetPKCERefreshTokenResponse(string clientId, string refreshToken)
    {
        try
        {
            return await new OAuthClient().RequestToken(new PKCETokenRefreshRequest(clientId, refreshToken));
        }
        catch (APIException)
        {
            return null;
        }
    }
}
