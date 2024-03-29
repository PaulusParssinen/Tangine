﻿namespace Tangine.Habbo;

[Flags]
public enum GamePatches
{
    None = 0,

    InjectEndPoint = 1,
    InjectRSAKeys = 2,

    InjectKeyShouter = 4,
    InjectEndPointShouter = 8,

    DisableHostChecks = 16,
    DisableEncryption = 32
}