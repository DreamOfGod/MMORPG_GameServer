using System.Configuration;

public static class DBConn
{
    private static string m_MMORPG_GameServer;

    public static string MMORPG_GameServer
    {
        get
        {
            if(string.IsNullOrEmpty(m_MMORPG_GameServer))
            {
                m_MMORPG_GameServer = ConfigurationManager.ConnectionStrings["MMORPG_GameServerDB"].ConnectionString;
            }
            return m_MMORPG_GameServer;
        }
    }
}