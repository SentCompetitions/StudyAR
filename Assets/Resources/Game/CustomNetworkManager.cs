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

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        AlertManager.ShowAlert(new Alert
        {
            title = "Соеденение разорванно",
            text = "Соеденение с сервером неожиданно разорванно",
            buttonText = "OK",
            onButtonClick = delegate {  }
        });

        Menu.instance.Exit();
    }
}
