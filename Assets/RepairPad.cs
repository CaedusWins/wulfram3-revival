using Com.Wulfram3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairPad : MonoBehaviour {

    private GameManager gameManager;

	void Awake ()
    {
        gameManager = FindObjectOfType<GameManager>();
	}


    // A click on the respawn map. Only your own team's pads build you a tank: any pad used to,
    // the enemy's included (design/gdd/win-condition.md).
    void OnMouseDown()
    {
        if (PlayerMovementManager.LocalPlayerInstance == null)
        {
            return;
        }
        PlayerMovementManager player = PlayerMovementManager.LocalPlayerInstance.GetComponent<PlayerMovementManager>();
        if (!BuildsFor(player))
        {
            return;
        }
        Spawn(gameManager, player, gameObject.transform.position);
    }

    // The pad's team (its Unit); "" for a pad that belongs to no one, like a placement preview.
    public string Team
    {
        get
        {
            Unit unit = GetComponent<Unit>();
            return unit == null ? "" : unit.team;
        }
    }

    public bool BuildsFor(PlayerMovementManager player)
    {
        Unit playerUnit = player == null ? null : player.GetComponent<Unit>();
        return playerUnit != null && Unit.NormalizeTeam(Team) != "" && Unit.SameTeam(Team, playerUnit.team);
    }

    public static int Count(string team)
    {
        int count = 0;
        foreach (RepairPad pad in FindObjectsOfType<RepairPad>())
        {
            if (Unit.NormalizeTeam(pad.Team) != "" && Unit.SameTeam(pad.Team, team))
            {
                count++;
            }
        }
        return count;
    }

    // The player's own team's pad nearest to the tank, or null when the team has none.
    public static RepairPad FindNearestOwn(PlayerMovementManager player)
    {
        RepairPad nearest = null;
        foreach (RepairPad pad in FindObjectsOfType<RepairPad>())
        {
            if (pad.BuildsFor(player) && (nearest == null ||
                Vector3.Distance(pad.transform.position, player.transform.position) < Vector3.Distance(nearest.transform.position, player.transform.position)))
            {
                nearest = pad;
            }
        }
        return nearest;
    }

    public static void Spawn(GameManager gameManager, PlayerMovementManager player, Vector3 position)
    {
        if (player.isDead)
        {
            gameManager.normalCamera.enabled = true;
            gameManager.overheadCamera.enabled = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            player.photonView.RPC("SetPosAndRotation", PhotonTargets.All, position + new Vector3(0, 5, 0), Quaternion.identity);

            HitPointsManager hitpointsManager = player.GetComponent<HitPointsManager>();
            hitpointsManager.TellServerHealth(hitpointsManager.maxHealth);

            player.GetComponent<FuelManager>().ResetFuel();
        }
    }
}
