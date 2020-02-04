using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupController : MonoBehaviour
{
    public GameObject pickUpPoint;
    public BoxCollider box;

    private ParticleSystem psHeart;
    [SerializeField]private GameObject m_HeartLight;
    private bool m_HeartStarted = false;
    public GameObject psHeartgm;

    public bool hugCube = false;
    public bool permanentHugCube = false;
    private float timeBeforeLoseHug = 0.5f;
    private float currentTimeBeforeLoseHug = 0;
    private bool m_CanHug = true;
    public bool canHugOwner = true;

    [HideInInspector] public float x_dist = 0;
    [HideInInspector] public float y_dist = 0;
    public float baseRaycastLength = 2.1f;

    [HideInInspector] public Rigidbody rb;

    [HideInInspector] public PlayerController lastOwner;
    protected Vector3 m_Forces;
    public bool owned = false;

    public Transform forwardRaycastHolder;
    public Transform downRaycastHolder;
    public LayerMask forwardMask;
    public LayerMask downMask;
    public LayerMask groundMask;

    private void Awake()
    {
        x_dist = transform.position.x - pickUpPoint.transform.position.x;
        y_dist = transform.position.y - pickUpPoint.transform.position.y;

        rb = GetComponent<Rigidbody>();
        box = GetComponent<BoxCollider>();

        psHeart = Instantiate(psHeartgm, transform).GetComponent<ParticleSystem>();
        psHeart.Stop();
    }

    private void FixedUpdate()
    {
        if (hugCube && !m_HeartStarted)
        {
            psHeart.Play();
            m_HeartStarted = true;
            if(m_HeartLight && !m_HeartLight.activeSelf)
                m_HeartLight.SetActive(true);

        }
        else if(!hugCube && m_HeartStarted)
        {
            psHeart.Stop();
            if(m_HeartLight && m_HeartLight.activeSelf)
                m_HeartLight.SetActive(false);
        }

        if (hugCube)
        {
            // psHeart.gameObject.transform.localPosition = Vector3.zero;
        }
        if (owned)
        {
            CheckForCollisionForward();
            CheckForCollisionDown();
        }
        if (owned)
        {
            currentTimeBeforeLoseHug = 0;
            canHugOwner = false;
        }
        else
        {
            currentTimeBeforeLoseHug += Time.deltaTime;
            if (currentTimeBeforeLoseHug >= timeBeforeLoseHug)
            {
                canHugOwner = true;
            }
        }
    }

    private void CheckForCollisionForward()
    {
        if (box.enabled)
            for (int i = 0; i < forwardRaycastHolder.transform.childCount; i++)
            {
                //float mult = lastOwner.m_LookRight ? baseRaycastLength : -baseRaycastLength;
                float mult = baseRaycastLength;
                Vector3 dir = lastOwner.m_LookRight ? transform.right : -transform.right;
                Debug.DrawRay(forwardRaycastHolder.transform.GetChild(i).transform.position, dir * mult, Color.red);
                RaycastHit hit;
                if (Physics.Raycast(forwardRaycastHolder.transform.GetChild(i).transform.position, dir, out hit, mult, forwardMask))
                {
                    if (hit.transform.gameObject == gameObject)
                    {
                        return;
                    }
                    if (hit.transform.gameObject.CompareTag("Player"))
                    {
                    }
                    float movementPlayer = InputManager.instance.CheckAxis(lastOwner.player);
                    lastOwner.WarpBack(-movementPlayer);

                    //Debug.Log("Hit something");
                    break;
                }
            }
    }

    private void CheckForCollisionDown()
    {
        if (box.enabled)
            for (int i = 0; i < downRaycastHolder.transform.childCount; i++)
            {
                //float mult = lastOwner.m_LookRight ? baseRaycastLength : -baseRaycastLength;
                float mult = baseRaycastLength;
                Vector3 dir = -transform.up;
                Debug.DrawRay(downRaycastHolder.transform.GetChild(i).transform.position, dir * baseRaycastLength, Color.green);
                RaycastHit hit;

                if (Physics.Raycast(downRaycastHolder.transform.GetChild(i).transform.position, dir, out hit, baseRaycastLength, downMask))
                {
                    if (hit.transform.gameObject != lastOwner.gameObject
                        && hit.transform.gameObject != gameObject)
                    {
                        lastOwner.Bump();
                        break;
                    }
                }
            }
    }

    public bool CheckTurnAround()
    {
        // for (int i = 0; i < forwardRaycastHolder.transform.childCount; i++)
        // {
        //     //float mult = lastOwner.m_LookRight ? baseRaycastLength : -baseRaycastLength;
        //     float mult = baseRaycastLength;
        //     Vector3 dir = lastOwner.m_LookRight ? transform.right : -transform.right;
        //     if (Physics.Raycast(forwardRaycastHolder.transform.GetChild(i).transform.position, dir, mult, forwardMask))
        //     {
        //         //Debug.Log("Hit something");
        //         return true;
        //     }
        // }
        // return false;
        return false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hugCube)
        {
            if (!permanentHugCube)
            {
                if (collision.gameObject != lastOwner.gameObject && collision.gameObject.tag == "Player")
                {
                    if (m_CanHug)
                    {
                        PlayerController collP = collision.gameObject.GetComponent<PlayerController>();
                        collP.GetHug();
                        StartCoroutine(WaitForHug());
                    }
                }

                if (collision.gameObject.tag == "sol")
                {
                    hugCube = false;
                }
            }
            else
            {
                bool last = canHugOwner ? true : collision.gameObject != lastOwner.gameObject;

                if (last && collision.gameObject.tag == "Player")
                {
                    if (m_CanHug)
                    {
                        PlayerController collP = collision.gameObject.GetComponent<PlayerController>();
                        collP.GetHug();
                        StartCoroutine(WaitForHug());
                    }
                }


            }
        }
    }

    private IEnumerator WaitForHug()
    {
        float time = 3;
        float curr = 0;
        m_CanHug = false;

        while (curr < time)
        {
            curr += Time.deltaTime;
            yield return null;
        }
        m_CanHug = true;
    }

    public void StartVFX()
    {
        psHeart.Play();
        m_HeartStarted = true;
        if(m_HeartLight && !m_HeartLight.activeSelf)
            m_HeartLight.SetActive(true);
    }

}
