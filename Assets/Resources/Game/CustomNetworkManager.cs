using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);

        if (maxConnections == 0) conn.Disconnect();
    }
}
