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


        if (playerManager.GetComponent<PlayerControllerManager>())
        {
            GUILayout.Space(20);

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
            GUILayout.Space(20);

            if (GUILayout.Button("Add Player Movement"))
            {
                if (!playerManager.GetComponent<PlayerControllerManager>())
                {
                    playerManager.playerControllerManager = playerManager.gameObject.AddComponent<PlayerControllerManager>();
                }
            }
        }

        if (!playerManager.GetComponent<PlayerAnimationManager>())
        {
            GUILayout.Space(20);

            if (GUILayout.Button("Add Player Animations"))
            {
                if (!playerManager.GetComponent<PlayerAnimationManager>())
                {
                    playerManager.playerAnimationManager = playerManager.gameObject.AddComponent<PlayerAnimationManager>();
                }
            }
        }
    }
}