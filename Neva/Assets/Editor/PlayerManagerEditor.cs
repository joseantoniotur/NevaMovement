using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerManager))]
public class PlayerManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector first
        DrawDefaultInspector();

        PlayerManager playerManager = (PlayerManager)target;

        GUILayout.Space(20);

        if (playerManager.GetComponent<PlayerControllerManager>())
        {
            if (!playerManager.GetComponent<BasicMovement>())
            {
                if (GUILayout.Button("Add Basic Movement"))
                {
                    playerManager.playerControllerManager.playerMovementList.Add(playerManager.gameObject.AddComponent<BasicMovement>());
                }
            }

            if (!playerManager.GetComponent<JumpMovement>())
            {
                if (GUILayout.Button("Add Jump"))
                {
                    playerManager.playerControllerManager.playerMovementList.Add(playerManager.gameObject.AddComponent<JumpMovement>());
                }
            }

            if (!playerManager.GetComponent<DodgeMovement>())
            {
                if (GUILayout.Button("Add Dodge"))
                {
                    playerManager.playerControllerManager.playerMovementList.Add(playerManager.gameObject.AddComponent<DodgeMovement>());
                }
            }
        }
        else 
        {
            if (GUILayout.Button("Add Player Movement"))
            {
                if (!playerManager.GetComponent<PlayerControllerManager>())
                {
                    playerManager.playerControllerManager = playerManager.gameObject.AddComponent<PlayerControllerManager>();
                }
            }
        }
    }
}