using Steamworks;

public static class PhotonSerializers
{
    public static object DeserializeCSteamID (byte[] data)
    {
        var result = new CSteamID(System.BitConverter.ToUInt64(data));
        return result;
    }

    public static byte[] SerializeCSteamID (object customType)
    {
        var result = (CSteamID)customType;
        return System.BitConverter.GetBytes(result.m_SteamID);
    }
}
