using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerManager : SingletonBehaviour<PlayerManager>
{
    [SerializeField, FoldoutGroup("Data")] private ObjectPool<PlayerController> m_PlayerPool;
    public static List<PlayerController> players => instance.m_PlayerPool.rentedObjects.Length > 0 ? instance.m_PlayerPool.rentedObjects.ToList() : new List<PlayerController>();
    public List<BuffData> m_Buffs = new List<BuffData>();
    public PlayerTracker[] m_PlayerTrackers = new PlayerTracker[4];

    private List<List<BuffData>> m_PlayerBuffs = new List<List<BuffData>>();

    public List<GameObject> chapeau;
    public List<Material> playerMat;

    private bool m_FirstPlayer = true;

    private int chapCount = 0;

    public void AddPlayer()
    {
        var count = m_FirstPlayer ? 0 : players.Count;
        m_FirstPlayer = false;
        var createdPlayer = m_PlayerPool.Rent();
        createdPlayer.player = (PlayerInput) count;
        if (count == 0 || count == 2)
            createdPlayer.team = 0;
        else
            createdPlayer.team = 1;
        createdPlayer.gameObject.layer = (10 + count);



        GameObject cha = chapeau[chapCount];
        if (cha)
        {
            createdPlayer.GiveHat(cha);
        }
        SkinnedMeshRenderer sk = createdPlayer.m_Mesh.GetComponent<SkinnedMeshRenderer>();
        // Debug.Log( sk.material);
        sk.material = playerMat[chapCount];
        //Transform parentp = createdPlayer.m_Mesh.transform.parent;
        //Destroy(createdPlayer.m_Mesh);

        //createdPlayer.m_Mesh = Instantiate(playerMat[chapCount], parentp);
        chapCount++;

        createdPlayer.idInManager = count;
        m_PlayerTrackers[count] = new PlayerTracker();
        m_PlayerBuffs.Add(new List<BuffData>());
        if(GlobalUIManager.instance)
        GlobalUIManager.instance.AddScoreCard(createdPlayer);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddBuff(new BuffData
            {
                methodImpact = BuffStyle.add,
                statToImpact = Stats.Speed,
                value = 5
            }, 0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddBuff(new BuffData
            {
                methodImpact = BuffStyle.multiply,
                statToImpact = Stats.Speed,
                value = -5
            }, 0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AddBuff(new BuffData
            {
                methodImpact = BuffStyle.add,
                statToImpact = Stats.Jump,
                value = 5
            }, 0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            AddBuff(new BuffData
            {
                methodImpact = BuffStyle.multiply,
                statToImpact = Stats.Jump,
                value = -5
            }, 0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            AddBuff(new BuffData
            {
                methodImpact = BuffStyle.add,
                statToImpact = Stats.Throw,
                value = 5
            }, 0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            AddBuff(new BuffData
            {
                methodImpact = BuffStyle.multiply,
                statToImpact = Stats.Throw,
                value = -5
            }, 0);
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Debug.Log("Buff Applied");
            ApplyBuff();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Buff Applied");
            ResetBuff();
        }
    }

    public void AddBuff(BuffData buff, PlayerController player)
    {
        int id = player.idInManager;
        List<BuffData> buffs = m_PlayerBuffs[id];

        buffs.Add(buff);
    }

    public void AddBuff(BuffData buff, int team)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].team == team)
            {
                int id = players[i].idInManager;
                List<BuffData> buffs = m_PlayerBuffs[id];

                buffs.Add(buff);
            }
        }
    }

    public void AddBuffOther(PlayerController player)
    {
        BuffData buff = m_Buffs[Random.Range(0, m_Buffs.Count)];
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] != player)
            {
                int id = players[i].idInManager;
                List<BuffData> buffs = m_PlayerBuffs[id];

                buffs.Add(buff);
            }
        }
        Debug.Log("buffed " + buff.name);
        ApplyBuff();
    }

    public void ApplyBuff()
    {
        for (int i = 0; i < players.Count; i++)
        {
            PlayerController p = players[i];
            p.ApplyBuffs(m_PlayerBuffs[i]);
            m_PlayerBuffs[i].Clear();
        }
    }

    public void ResetBuff()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].ResetBuff();
        }
    }

    public void TrackerUpdate(PlayerController player, PlayerTrackerVariable tracker)
    {
        int id = player.idInManager;
        if (id != -1)
        {
            switch (tracker)
            {
                case PlayerTrackerVariable.hugs:
                    m_PlayerTrackers[id].hugs += 1;
                    break;
                case PlayerTrackerVariable.build:
                    m_PlayerTrackers[id].builder += 1;
                    break;
                case PlayerTrackerVariable.rogue:
                    m_PlayerTrackers[id].rogue += 1;
                    break;
                case PlayerTrackerVariable.jump:
                    m_PlayerTrackers[id].jumper += 1;
                    break;
                case PlayerTrackerVariable.win:
                    m_PlayerTrackers[id].winner += 1;
                    break;
                case PlayerTrackerVariable.thrower:
                    m_PlayerTrackers[id].thrower += 1;
                    break;
            }
        }
    }

    public void RevivePlayers()
    {
        MiniGameController.RepositionDeadPlayer();
        foreach (var player in players)
        {
            player.gameObject.SetActive(true);
        }
    }
}

public struct PlayerTracker
{
    public int hugs;
    public int builder;
    public int rogue;
    public int jumper;
    public int winner;
    public int thrower;
}

public enum PlayerTrackerVariable
{
    hugs,
    build,
    rogue,
    jump,
    win,
    thrower
}
