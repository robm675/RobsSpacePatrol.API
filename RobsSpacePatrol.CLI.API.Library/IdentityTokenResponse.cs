using System;
using System.Collections.Generic;
using System.Text;

namespace RobsSpacePatrol.CLI.API.Library;

public sealed class IdentityTokenResponse
{
    public string TokenType { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public long ExpiresIn { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}