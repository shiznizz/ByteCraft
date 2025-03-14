using Unity.VisualScripting;
using UnityEngine;

public class grappleGun : MonoBehaviour
{
    [Header("References")]
    public LineRenderer lr;
    public Transform gunTip, cam, player;
    [SerializeField] LayerMask maskGrappable;

    [Header("Fine Tune")]
    [SerializeField] float maxDistFromGrapple;
    [SerializeField] float minDistFromGrapple;

    [SerializeField] float jointSpring;
    [SerializeField] float jointDamper;
    [SerializeField] float jointMassScale;

    [SerializeField] float maxGrappleDistance;

    //[Header("Swing Movement")]
    //public Transform orientation;
    //public Rigidbody rb;
    //public float horizontalThrustForce;
    //public float forwardThrustForce;
    //public float backwardsThrustForce;
    // public float extendCableSpeed;


    private Vector3 grapplePoint;
    private SpringJoint joint;

    //[Header("Prediction")]
    //public RaycastHit predictionHit;
    //public float predictionSphereCastRadius;
    //public Transform predictionPoint;

    [Header("Input")]
    [SerializeField] KeyCode grappleKey;
    [SerializeField] KeyCode jumpKey;

    private void Update()
    {
        if (Input.GetKeyDown(grappleKey)) startGrapple();
        if (Input.GetKeyDown(jumpKey)) stopGrapple();
    }

    private void LateUpdate()
    {
        drawRope();
    }

    private void startGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, maskGrappable))
        {
            grapplePoint = hit.point;
            // adds a springjoint to the player
            joint = player.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            // sets the anchor 
            joint.connectedAnchor = grapplePoint;

            float distFromGrapplePoint = Vector3.Distance(player.position, grapplePoint);

            joint.maxDistance = distFromGrapplePoint * maxDistFromGrapple;
            joint.minDistance = distFromGrapplePoint * minDistFromGrapple;

            joint.spring = jointSpring;
            joint.damper = jointDamper;
            joint.massScale = jointMassScale;

            lr.positionCount = 2;
        }
    }

    private void drawRope()
    {
        if (!joint) return;

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, grapplePoint);
    }

    private void stopGrapple()
    {
        lr.positionCount = 0;

        Destroy(joint);
    }
}
