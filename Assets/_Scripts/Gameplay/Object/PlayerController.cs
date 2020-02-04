using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Variables

    public PlayerInput player;
    public EntityData data;
    public int team;
    public int idInManager = 0;

    private Rigidbody m_RB;
    [SerializeField] private Transform m_BodyRaycaster;
    [SerializeField] private float m_BodyRaycasterLenght, m_FeetRaycasterLenght;
    private Vector3 m_MoveDir = new Vector3();
    protected Vector3 m_Forces;
    public Transform m_Rig;

    [Header("Movement")] public float m_PlayerCurrentSpeed = 10f;
    private float m_CurrentMovement = 0;
    [HideInInspector] public bool m_LookRight = true;

    [Header("Jump")] public float m_PlayerCurrentJumpForce = 10f;
    public float m_TimeJump = 0.5f;
    public GameObject raycastParent;
    public LayerMask jumpMask;
    private float m_CurrentTimeJump = 0;

    [Header("Interact")] public GameObject grabBox;
    public Transform hand;
    public Transform head;
    public LayerMask grabMask;
    public LayerMask toGrab;
    public float m_PlayerCurrentMaxThrow = 10f;
    private GameObject m_CurrentCube;
    private PickupController m_CurrentCubeScript;

    //Throw
    private float buildUp = 0;
    private GameObject m_BuildUpOB;
    private ParticleSystem m_BuildUpPS;
    private GameObject m_AirOB;
    private ParticleSystem m_AirPS;
    private bool throwBuildUpVFXStarted = false;

    [Header("Hug")] public LayerMask hugMask;
    private bool m_InHug = false;

    [Header("Bump")] public float bumpStr = 1;
    public float timeBeforeBump = 0.5f;
    private bool canBump = true;

    public List<ItemData> items = new List<ItemData>();
    private float TimeBeforeBuff = 15;
    private float currentTimeBeforeBuff = 0;

    //VFX


    //Buffs
    private List<BuffData> m_BuffDatas = new List<BuffData>();
    private float m_LensThrowAngle;
    private ParticleSystem m_Arrow;
    private GameObject m_ArrowContainer;


    /// <summary>
    /// Variable bool to control gameplay
    /// </summary>
    private bool m_BuildingUp = false;

    private Animator m_Anim;

    public GameObject m_Mesh;

    [Header("Debug")] public float debugMove;
    public float anglex;
    public float angley;

    [SerializeField] private Collider m_Collider;
    public LayerMask m_PlatformLayer;

    #endregion

    /// <summary>
    /// Collision check non alloc
    /// Sphere cast non alloc
    /// </summary>
    private void Awake()
    {
        team = 0;
        m_Anim = GetComponentInChildren<Animator>();
        m_RB = gameObject.GetComponent<Rigidbody>();

        m_CurrentTimeJump = m_TimeJump;
        m_PlayerCurrentJumpForce = data.baseJumpStrenght;
        m_PlayerCurrentSpeed = data.baseMovementSpeed;
        m_PlayerCurrentMaxThrow = data.baseMaxThrowStrenght;

        if (data.throwBuildUpVFX)
        {
            if (m_BuildUpOB == null)
                m_BuildUpOB = Instantiate(data.throwBuildUpVFX.gameObject, this.transform);
            m_BuildUpOB.transform.localPosition = new Vector3(0, -0.5f, 0);
            m_BuildUpPS = m_BuildUpOB.GetComponentInChildren<ParticleSystem>();
            m_BuildUpPS.Stop();
        }

        if (data.airVFX)
        {
            if (m_AirOB == null)
                m_AirOB = Instantiate(data.airVFX.gameObject, this.transform);
            m_AirOB.transform.localPosition = new Vector3(0, -0.5f, 0);
            m_AirPS = m_AirOB.GetComponentInChildren<ParticleSystem>();
            m_AirPS.Stop();
        }

        //if (data.heartVFX)
        //{
        //    if (m_HeartOBJ == null)
        //        m_HeartOBJ = Instantiate(data.heartVFX.gameObject, head.transform);
        //    m_HeartOBJ.transform.localPosition = new Vector3(0, 0.5f, 0);
        //    m_HeartVFX = m_HeartOBJ.GetComponentInChildren<ParticleSystem>();
        //    m_HeartVFX.Stop();
        //}
    }

    private void Start()
    {
        /*ParticleSystem pa = Instantiate(data.arrowVFX).GetComponentInChildren<ParticleSystem>();
        pa.Play();*/
    }

    private void Update()
    {
        if (!m_InHug)
        {
            if (InputManager.instance.CheckInput(player, ControllerInput.Jump, MethodInput.Down))
            {
                if (Ground())
                    InitialJump();
            }

            if (InputManager.instance.CheckInput(player, ControllerInput.Hug, MethodInput.Down))
            {
                Hug();
            }

            if (m_CurrentCube == null)
            {
                if (InputManager.instance.CheckInput(player, ControllerInput.Interact, MethodInput.Up))
                {
                    Interact();
                }
            }
            else
            {
                if (InputManager.instance.CheckInput(player, ControllerInput.Interact, MethodInput.Hold))
                {
                    m_BuildingUp = true;

                    BuildThrow();
                }

                if (InputManager.instance.CheckInput(player, ControllerInput.Interact, MethodInput.Up))
                {
                    Throw();
                }
            }
        }

        if (items.Find(x => x.ItemType == ItemType.Boost))
        {
            currentTimeBeforeBuff += Time.deltaTime;
            if (currentTimeBeforeBuff >= TimeBeforeBuff)
            {
                currentTimeBeforeBuff = 0;
                PlayerManager.instance.ResetBuff();
                PlayerManager.instance.AddBuffOther(this);
            }
        }


        if (m_CurrentCube)
        {
            m_CurrentCube.transform.localPosition = Vector3.zero;
        }
    }

    private void OnEnable()
    {
        if (m_BuildUpPS)
            m_BuildUpPS.Stop();
    }

    private void FixedUpdate()
    {
        if (!m_InHug)
        {
            if (InputManager.instance.CheckInput(player, ControllerInput.Jump, MethodInput.Hold))
            {
                if (items.Find(x => x.ItemType == ItemType.Boots) && m_RB.velocity.y < 0)
                {
                    if (!m_AirPS.isPlaying)
                    {
                        m_AirPS.Play();
                    }

                    Hover();
                }
                else if (m_CurrentTimeJump > 0)
                {
                    if (m_RB.velocity.y > 0)
                    {
                        Jump();
                        m_CurrentTimeJump -= Time.deltaTime;
                    }
                }
            }
            else
            {
                if (m_AirPS && m_AirPS.isPlaying)
                {
                    // if(Physics.GetIgnoreLayerCollision(8,10))
                    //     Physics.IgnoreLayerCollision(8,10, false);
                    m_AirPS.Stop();
                }
            }

            Move();
            // StartCoroutine(WaitforEndOfMove());
            // Move();
            m_MoveDir.y = m_RB.velocity.y;
        }

        Fall();


        m_MoveDir += m_Forces;
        m_MoveDir.x = Mathf.Clamp(m_MoveDir.x, -data.maxMovementSpeed, data.maxMovementSpeed);
        m_RB.velocity = m_MoveDir;

        m_Forces = Vector3.Lerp(m_Forces, Vector3.zero, Time.deltaTime * 1.5f);

        m_CurrentVelocity = m_RB.velocity;
        ResetForce();
    }


    #region Jump

    private bool m_inTheAir = false;

    private void InitialJump()
    {
        GameAudioManager.instance.PlaySoundOneShot(AudioEventType.Jump, transform.position);
        PlayerManager.instance.TrackerUpdate(this, PlayerTrackerVariable.jump);
        m_CurrentTimeJump = data.maxJumpTime;
        //Debug.Log("Player " + player + " has jump");
        m_RB.AddForce(Vector3.up * m_PlayerCurrentJumpForce, ForceMode.Impulse);
        m_inTheAir = true;
    }

    private void Jump()
    {
        Physics.IgnoreLayerCollision(8, (10 + idInManager), true); //IgnoreCollision(other.collider, m_Collider, true);
        m_RB.AddForce(Vector3.up * data.jumpForceOverTime);
    }

    private void Hover()
    {
        m_RB.AddForce(Vector3.up * data.jumpForceOverTime * data.hoverStrenght);
    }

    private void Fall()
    {
        if (m_RB.velocity.y < 0 && Physics.GetIgnoreLayerCollision(8, (10 + idInManager)))
            Physics.IgnoreLayerCollision(8, (10 + idInManager), false);
        if (!Ground() && m_RB.velocity.y >= -30)
        {
            m_RB.AddForce(new Vector2(0, data.gravityForce));
            m_MoveDir.y = m_RB.velocity.y;
        }
        else
        {
            m_RB.AddForce(new Vector2(0, data.gravityForce));
            m_MoveDir.y = m_RB.velocity.y;

            m_RB.velocity = new Vector3(m_RB.velocity.x, m_RB.velocity.y, 0);
        }
    }

    private bool Ground()
    {
        var m_bounced = false;
        for (int i = 0; i < raycastParent.transform.childCount; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(raycastParent.transform.GetChild(i).transform.position, -transform.up, out hit, 0.6f, jumpMask))
            {
                if (m_inTheAir)
                {
                    m_inTheAir = false;
                    m_Anim.SetTrigger("land");
                }

                return true;
            }
            else
            {
                RaycastHit otherHit;
                var lateralVector = transform.right;
                lateralVector *= m_FeetRaycasterLenght;
                if (!m_bounced && i == 1)
                    Debug.DrawRay(raycastParent.transform.GetChild(i).transform.position, lateralVector, Color.green);
                if (!m_bounced && i == 1 && Physics.Raycast(raycastParent.transform.GetChild(i).transform.position, lateralVector, out otherHit, m_FeetRaycasterLenght, jumpMask))
                {
                    m_bounced = true;
                    Debug.Log("Feet Bounced Right");
                    m_RB.velocity = Vector3.zero;
                    m_MoveDir = Vector3.zero;
                    m_RB.AddForce(-Vector3.right * 50);
                }
                else
                {
                    if (!m_bounced && i == 1)
                        Debug.DrawRay(raycastParent.transform.GetChild(i).transform.position, -lateralVector, Color.green);
                    if (!m_bounced && i == 1 && Physics.Raycast(raycastParent.transform.GetChild(i).transform.position, -lateralVector, out otherHit, m_FeetRaycasterLenght,
                            jumpMask))
                    {
                        m_bounced = true;
                        var newX = m_MoveDir.x;
                        if (newX < 0)
                        {
                            newX = -newX;
                        }

                        var newDir = new Vector3(newX, m_MoveDir.y, 0);
                        m_MoveDir = newDir;
                        m_RB.velocity = newDir;
                        Debug.Log("Feet Bounced Left");
                        // m_RB.velocity = Vector3.zero;
                        // m_MoveDir = Vector3.zero;
                        // m_RB.AddForce(Vector3.right *  50);
                    }
                    else
                    {
                        lateralVector = transform.right;
                        lateralVector *= m_BodyRaycasterLenght;
                        if (!m_bounced && i == 1)
                            Debug.DrawRay(m_BodyRaycaster.position, lateralVector, Color.green);
                        if (!m_bounced && i == 1 && Physics.Raycast(m_BodyRaycaster.position, lateralVector, out otherHit, m_BodyRaycasterLenght, jumpMask))
                        {
                            m_bounced = true;
                            Debug.Log("Body Bounced Right");
                            m_RB.velocity = Vector3.zero;
                            m_MoveDir = Vector3.zero;
                            m_RB.AddForce(-Vector3.right * 50);
                        }
                        else
                        {
                            if (!m_bounced && i == 1)
                                Debug.DrawRay(m_BodyRaycaster.position, -lateralVector, Color.green);
                            if (!m_bounced && i == 1 && Physics.Raycast(m_BodyRaycaster.position, -lateralVector, out otherHit, m_BodyRaycasterLenght, jumpMask))
                            {
                                m_bounced = true;
                                var newX = m_MoveDir.x;
                                if (newX < 0)
                                {
                                    newX = -newX;
                                }

                                var newDir = new Vector3(newX, m_MoveDir.y, 0);
                                m_MoveDir = newDir;
                                m_RB.velocity = newDir;
                                Debug.Log("Body Bounced Left");
                                m_RB.velocity = Vector3.zero;
                                m_MoveDir = Vector3.zero;
                                m_RB.AddForce(Vector3.right * 50);
                            }
                        }
                    }
                }
            }


            Debug.DrawRay(raycastParent.transform.GetChild(i).transform.position, -transform.up * 0.6f, Color.red);
            //if (player == PlayerInput.P1)
            //{
            //    RaycastHit hit;
            //    Physics.Raycast(raycastParent.transform.GetChild(i).transform.position, -transform.up, out hit, 1f);
            //    if(hit.transform)
            //        Debug.Log(hit.transform.name);
            //}
        }

        return false;
    }

    #endregion

    private void Hug()
    {
        if (m_CurrentCube == null)
        {
            GameAudioManager.instance.PlaySoundOneShot(AudioEventType.Hug, transform.position);
            grabBox.SetActive(true);
            Collider[] touched = new Collider[16];
            int stored = Physics.OverlapBoxNonAlloc(grabBox.transform.position, grabBox.transform.localScale, touched, grabBox.transform.rotation, hugMask,
                QueryTriggerInteraction.Collide);
            if (stored != 0)
            {
                for (int i = 0; i < stored; i++)
                {
                    PlayerController hugged = touched[i].GetComponent<PlayerController>();
                    if (hugged == this) continue;
                    if (hugged.m_InHug) continue;

                    m_Anim.SetTrigger("Hug");

                    if (m_LookRight)
                    {
                        m_Rig.position = new Vector3(hugged.transform.position.x - 0.25f, m_Rig.position.y);
                    }
                    else
                    {
                        m_Rig.position = new Vector3(hugged.transform.position.x + 0.25f, m_Rig.position.y);
                    }

                    PlayerManager.instance.TrackerUpdate(this, PlayerTrackerVariable.hugs);
                    m_RB.velocity = new Vector3(0, m_RB.velocity.y);
                    m_MoveDir = new Vector3(0, m_MoveDir.y);
                    m_InHug = true;
                    gameObject.GetComponent<Renderer>().material.SetFloat("Albedo", 98324);
                    //hugged.m_InHug = true;
                    StartCoroutine(HugStun(HugState.Giver));
                    hugged.GetHug();
                    GameObject h = Instantiate(data.heartVFX);
                    h.transform.position = head.transform.position;


                    break;
                }

                //hugged.StartCoroutine(HugStun(HugState.Receiver));
            }

            grabBox.SetActive(false);
        }
    }

    public void GetHug()
    {
        m_Anim.SetTrigger("getHug");

        m_InHug = true;
        m_RB.velocity = new Vector3(0, m_RB.velocity.y);
        m_MoveDir = new Vector3(0, m_MoveDir.y);
        if (m_CurrentCube)
        {
            Drop();
        }

        StartCoroutine(HugStun(HugState.Receiver));
    }

    private void Interact()
    {
        /*if (m_CurrentCube)
        {

            Throw();
            return;

        }*/
        //Debug.Log("Player " + player + " has interacted");
        grabBox.SetActive(true);
        GameAudioManager.instance.PlaySoundOneShot(AudioEventType.Grab, transform.position);
        Collider[] touched = new Collider[16];
        int stored = Physics.OverlapBoxNonAlloc(grabBox.transform.position, grabBox.transform.localScale / 2, touched, grabBox.transform.rotation, grabMask,
            QueryTriggerInteraction.Collide);

        if (stored != 0)
        {
            for (int i = 0; i < touched.Length; i++)
            {
                GameObject obj = touched[0].gameObject;
                //Debug.Log(LayerMask.LayerToName(obj.layer) + " " + toGrab.ToString());
                if (LayerMask.LayerToName(obj.layer) == "Cube")
                {
                    PickupController temp = obj.GetComponent<PickupController>();

                    if (!temp.owned)
                    {
                        //Debug.Log(obj.name);
                        buildUp = data.buildUpMin;
                        temp.owned = true;
                        m_CurrentCube = obj;
                        m_CurrentCubeScript = temp;
                        m_CurrentCubeScript.rb.isKinematic = true;
                        m_CurrentCube.transform.localEulerAngles = Vector3.zero;

                        if (items.Find(x => x.ItemType == ItemType.HugBlock))
                        {
                            m_CurrentCubeScript.hugCube = true;
                            m_CurrentCubeScript.StartVFX();
                        }

                        m_CurrentCubeScript.lastOwner = this;
                        m_CurrentCubeScript.box.enabled = false;
                        m_CurrentCube.transform.parent = hand.transform;
                        m_CurrentCube.transform.localPosition = Vector3.zero;

                        //if (m_LookRight)
                        //{
                        //   /* m_CurrentCube.transform.position =
                        //        grabBox.transform.position + new Vector3(m_CurrentCubeScript.x_dist + data.grapDistanceModifier, m_CurrentCubeScript.y_dist);*/
                        //    m_CurrentCube.transform.parent = hand.transform;
                        //    m_CurrentCube.transform.localPosition = Vector3.zero;
                        //}
                        //else
                        //{
                        //    //m_CurrentCube.transform.position =
                        //    //    grabBox.transform.position + new Vector3(-m_CurrentCubeScript.x_dist - data.grapDistanceModifier, m_CurrentCubeScript.y_dist);
                        //    //m_CurrentCube.transform.parent = grabBox.transform;

                        //}

                        m_CurrentCube.transform.localScale = new Vector3(Mathf.Abs(m_CurrentCube.transform.localScale.x), m_CurrentCube.transform.localScale.y,
                            m_CurrentCube.transform.localScale.z);
                    }

                    if (MiniGameController.current)
                        MiniGameController.current.HandleCallObjectGrabbed(temp, this);
                    break;
                }
            }
        }
        else
        {
            grabBox.SetActive(false);
        }
    }

    private void Throw()
    {
        if (m_CurrentCube)
        {
            GameAudioManager.instance.PlaySoundOneShot(AudioEventType.Throw, transform.position);
            if (MiniGameController.current)
                MiniGameController.current.HandleCallObjectDroped(m_CurrentCubeScript, this);

            m_CurrentCubeScript.rb.isKinematic = false;
            m_CurrentCubeScript.owned = false;
            m_CurrentCube.transform.parent = null;

            if (m_BuildUpPS && throwBuildUpVFXStarted)
            {
                throwBuildUpVFXStarted = false;
                m_BuildUpPS.Stop();
            }

            PlayerManager.instance.TrackerUpdate(this, PlayerTrackerVariable.thrower);
            float mult = m_LookRight ? 1 : -1;
            float angle;
            float angleY;
            if (m_LensThrowAngle != 0)
            {
                if (m_LensThrowAngle == 90)
                {
                    angle = 0;
                    angleY = 90;
                }
                else if (m_LensThrowAngle == 180 || m_LensThrowAngle == 0)
                {
                    angle = m_LensThrowAngle;
                    angleY = 0;
                }
                else if (m_LensThrowAngle < 90)
                {
                    angle = 90 - m_LensThrowAngle;
                    angleY = m_LensThrowAngle;
                }
                else
                {
                    angle = m_LensThrowAngle - 90;
                    angleY = 180 - m_LensThrowAngle;
                }

                anglex = angle;
                angley = angleY;

                //angle = anglex;
                //angleY = angley;

                angle /= 100;
                angleY /= 100;
            }
            else
            {
                angle = data.throwAngle / 100;
                angleY = 0.9f - angle;
            }

            if (m_Arrow)
            {
                m_Arrow.Stop();
                m_Arrow = null;
                Destroy(m_ArrowContainer);
            }

            m_Anim.SetTrigger("Throw");
            m_CurrentCube.transform.parent = grabBox.transform;
            m_CurrentCube.transform.position =
                grabBox.transform.position + new Vector3(m_CurrentCubeScript.x_dist + data.grapDistanceModifier, m_CurrentCubeScript.y_dist);
            m_CurrentCube.transform.parent = null;
            m_CurrentCubeScript.box.enabled = true;

            m_CurrentCubeScript.rb.AddForce(new Vector3(mult * angle, angleY) * buildUp, ForceMode.Impulse);
            //m_RB.AddForce((new Vector3(mult * angle, angleY) * buildUp * -1), ForceMode.Impulse);

            m_BuildingUp = false;

            m_CurrentCube = null;
            m_CurrentCubeScript = null;
            grabBox.SetActive(false);
        }
    }

    private void BuildThrow()
    {
        if (items.Find(x => x.ItemType == ItemType.Lens))
        {
            if (!m_Arrow)
            {
                m_ArrowContainer = data.arrowVFX ? Instantiate(data.arrowVFX, transform) : gameObject;
                m_Arrow = m_ArrowContainer.GetComponentInChildren<ParticleSystem>();
                m_Arrow.Play();
                m_ArrowContainer.transform.localPosition = Vector3.zero;
            }

            float x = InputManager.instance.CheckAxis(player);
            float y = InputManager.instance.CheckAxis(player, "Vertical_");
            if (y < 0)
            {
                m_LensThrowAngle = Mathf.Abs(Mathf.Atan2(y, x) * Mathf.Rad2Deg);
                if (m_LensThrowAngle < 90)
                {
                    m_ArrowContainer.transform.localEulerAngles = new Vector3(0, 0, m_LensThrowAngle);
                }
                else
                {
                    m_ArrowContainer.transform.localEulerAngles = new Vector3(0, 0, -m_LensThrowAngle);
                }
            }
            else
            {
                if (x < 0)
                {
                    m_LensThrowAngle = 180;
                }
                else
                {
                    m_LensThrowAngle = 0.1f;
                }

                if (m_LensThrowAngle < 90)
                {
                    m_ArrowContainer.transform.localEulerAngles = new Vector3(0, 0, m_LensThrowAngle);
                }
                else
                {
                    m_ArrowContainer.transform.localEulerAngles = new Vector3(0, 0, -m_LensThrowAngle);
                }
            }

            if (m_ArrowContainer.transform.lossyScale.x == -1)
            {
                m_ArrowContainer.transform.localScale = new Vector3(m_ArrowContainer.transform.localScale.x * -1, m_ArrowContainer.transform.localScale.y * -1,
                    m_ArrowContainer.transform.localScale.z);
            }
        }

        buildUp += data.buildUpRate;

        if (m_BuildUpPS && !throwBuildUpVFXStarted)
        {
            throwBuildUpVFXStarted = true;
            m_BuildUpPS.Play();
        }

        if (buildUp >= m_PlayerCurrentMaxThrow)
        {
            Throw();
        }
    }

    private void Move()
    {
        if (m_BuildingUp)
        {
            if (Ground())
                m_MoveDir.x = 0;

            float move = InputManager.instance.CheckAxis(player);

            Debug.Log("Check du buildup");
            if (Mathf.Abs(move) > 0.16f)
            {
                if (move < 0 && m_LookRight)
                {
                    transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                    m_LookRight = false;
                    if (m_CurrentCube)
                    {
                        if (m_CurrentCubeScript.CheckTurnAround())
                        {
                            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                            m_LookRight = true;
                            Debug.Log("Turn left cancel");
                        }
                    }
                }
                else if (move > 0 && !m_LookRight)
                {
                    transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                    m_LookRight = true;

                    if (m_CurrentCube)
                    {
                        if (m_CurrentCubeScript.CheckTurnAround())
                        {
                            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                            m_LookRight = false;
                            // Debug.Log("Turn right cancel");
                        }
                    }
                }
            }


            return;
        }

        m_CurrentMovement = InputManager.instance.CheckAxis(player);

        // if (Math.Abs(m_CurrentMovement) > 0.0001f)
        //     Debug.Log(m_CurrentMovement);
        if (Mathf.Abs(m_CurrentMovement) > 0.16f)
        {
            m_Anim.SetBool("walking", true);

            if (m_CurrentMovement < -0.1f && m_LookRight)
            {
                Debug.Log("Mouvement a gauche et regarde a droite");
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                m_LookRight = false;
                if (m_CurrentCube)
                {
                    if (m_CurrentCubeScript.CheckTurnAround())
                    {
                        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                        m_LookRight = true;
                    }
                }

                m_MoveDir.x *= -1; // m_CurrentMovement * m_PlayerCurrentSpeed;
                m_RB.velocity = new Vector3(m_MoveDir.x, m_RB.velocity.y, 0);
                return;
            }
            else if (m_CurrentMovement < -0.1f)
            {
                Debug.Log("Continue d'avoir le stick a gauche");
                m_MoveDir.x = m_CurrentMovement * m_PlayerCurrentSpeed;
                m_RB.velocity = new Vector3(m_MoveDir.x, m_RB.velocity.y, 0);
                return;
            }

            if (m_CurrentMovement > 0.1f && !m_LookRight)
            {
                Debug.Log("Mouvement a droite et regarde a gauche");
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                m_LookRight = true;
                if (m_CurrentCube)
                {
                    if (m_CurrentCubeScript.CheckTurnAround())
                    {
                        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                        m_LookRight = false;
                    }
                }

                m_MoveDir.x *= -1; // m_CurrentMovement * m_PlayerCurrentSpeed;
                m_RB.velocity = new Vector3(m_MoveDir.x, m_RB.velocity.y, 0);
                return;
            }
            else if (m_CurrentMovement > 0.1f)
            {
                Debug.Log("Continue d'avoir le stick a droite");
                m_MoveDir.x = m_CurrentMovement * m_PlayerCurrentSpeed;
                m_RB.velocity = new Vector3(m_MoveDir.x, m_RB.velocity.y, 0);
                return;
            }

            Debug.Log("Cas weird, remise a zero");
            // m_MoveDir.x = 0;
            m_MoveDir.x = m_CurrentMovement * m_PlayerCurrentSpeed;
            m_RB.velocity = new Vector3(m_MoveDir.x, m_RB.velocity.y, 0);

            // m_RB.velocity = new Vector3( m_MoveDir.x, m_RB.velocity.y,0);
            return;
        }
        else
        {
            if (Ground())
                m_MoveDir.x = 0;

            m_Anim.SetBool("walking", false);

            //Debug.Log("no move");
        }
    }

    public void ForceMove(Vector3 v)
    {
        if (Ground())
        {
            transform.position += v;
        }
    }

    public void WarpBack(float move)
    {
        if (m_BuildingUp)
        {
            if (!Ground())
            {
                m_Forces.x = -m_RB.velocity.x;
            }
            else
            {
                m_Forces.x = 0;
            }

            return;
        }

        m_Forces.x = move * m_PlayerCurrentSpeed;
    }

    public void ApplyBuffs(List<BuffData> buffs)
    {
        for (int i = 0; i < buffs.Count; i++)
        {
            BuffData bd = buffs[i];
            switch (bd.statToImpact)
            {
                case Stats.Jump:
                    m_PlayerCurrentJumpForce = bd.methodImpact == BuffStyle.add ? m_PlayerCurrentJumpForce + bd.value : m_PlayerCurrentJumpForce * bd.value;
                    break;
                case Stats.Speed:
                    m_PlayerCurrentSpeed = bd.methodImpact == BuffStyle.add ? m_PlayerCurrentSpeed + bd.value : m_PlayerCurrentSpeed * bd.value;
                    break;
                case Stats.Throw:
                    m_PlayerCurrentMaxThrow = bd.methodImpact == BuffStyle.add ? m_PlayerCurrentMaxThrow + bd.value : m_PlayerCurrentMaxThrow * bd.value;
                    break;
            }

            m_PlayerCurrentJumpForce = Mathf.Clamp(m_PlayerCurrentJumpForce, data.minJumpStrenght, data.maxJumpStrenght);
            m_PlayerCurrentSpeed = Mathf.Clamp(m_PlayerCurrentSpeed, data.minMovementSpeed, data.maxMovementSpeed);
            m_PlayerCurrentMaxThrow = Mathf.Clamp(m_PlayerCurrentMaxThrow, data.minThrowStrenght, data.maxThrowStrenght);
        }
    }

    public void ResetBuff()
    {
        m_PlayerCurrentJumpForce = data.baseJumpStrenght;
        m_PlayerCurrentSpeed = data.baseMovementSpeed;
        m_PlayerCurrentMaxThrow = data.baseMaxThrowStrenght;
    }

    protected void ResetForce()
    {
        if (m_RB.velocity.y <= 0 && Ground())
        {
            m_Forces.x = 0;
            m_Forces.y = 0;
        }
        else if (m_Forces.y >= 0)
        {
            m_Forces.x -= Time.deltaTime;
            m_Forces.y -= Time.deltaTime;
        }

        m_Forces.x = 0;
    }

    public void Bump()
    {
        if (canBump)
        {
            canBump = false;

            int direction = m_LookRight ? -01 : 1;
            m_RB.velocity = Vector3.zero;
            m_MoveDir = Vector3.zero;
            ResetForce();
            Vector3 v = new Vector3(direction * 0.80f * bumpStr, 0.45f * bumpStr);
            Debug.Log("velo " + v);
            m_RB.AddForce(v, ForceMode.Impulse);
            //m_Forces += v;
            StartCoroutine(BumpWait());
        }
    }

    private IEnumerator BumpWait()
    {
        float time = timeBeforeBump;
        while (timeBeforeBump > 0)
        {
            timeBeforeBump -= Time.deltaTime;
            yield return null;
        }

        canBump = true;

        timeBeforeBump = time;
    }

    private IEnumerator HugStun(HugState hug)
    {
        float maxTimer = hug == HugState.Giver ? data.timeHugAttack : data.timeHugDefend;
        float currentTime = 0;
        while (currentTime < maxTimer)
        {
            currentTime += Time.deltaTime;
            debugMove = currentTime;
            yield return null;
        }

        if (hug == HugState.Giver)
        {
            m_Rig.localPosition = new Vector3(0, m_Rig.localPosition.y);
        }

        m_InHug = false;
    }

    public void Drop()
    {
        float thr = m_PlayerCurrentMaxThrow;
        m_PlayerCurrentMaxThrow = 0;
        Throw();
        m_PlayerCurrentMaxThrow = thr;
    }

    public bool IsHoldingThisObject(PickupController itemToCheck)
    {
        return m_CurrentCubeScript.Equals(itemToCheck);
    }

    public void GiveHat(GameObject hat)
    {
        GameObject gm = Instantiate(hat, head.transform);
        gm.transform.localPosition = Vector3.zero;
    }

    private Vector3 m_CurrentVelocity;
    // private void OnCollisionEnter(Collision other)
    // {
    //     if (other.gameObject.CompareTag("sol"))
    //     {
    //         if(other.transform.position.y > transform.position.y)
    //         {
    //         }
    //         else
    //         {
    //             Physics.IgnoreCollision(other.collider, m_Collider, false);
    //         }
    //     }
    // }
    //
    // private void OnCollisionExit(Collision other)
    // {
    //     if (other.gameObject.CompareTag("sol"))
    //     {
    //         Debug.Log(m_CurrentVelocity);
    //         m_RB.velocity = m_CurrentVelocity;
    //     }
    // }
}

public enum HugState
{
    Giver,
    Receiver
}

public enum Items
{
    Lens,
    HugBlock,
    Boots,
}