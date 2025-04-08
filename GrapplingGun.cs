using UnityEngine;

public class GrapplingGun : MonoBehaviour {
private PlayerMovement playerMovement;
    private LineRenderer lr;
    private Vector3 grapplePoint;
    public LayerMask whatIsGrappleable;
    public LayerMask whatIsRed;
    public Transform gunTip, camera, player;
    private float maxDistance = 100f;
    private SpringJoint joint;

    private bool isGrappling = false;

  
    void Start() {
    playerMovement = FindObjectOfType<PlayerMovement>(); // Finds PlayerMovement in the scene
    }     
    
    void Awake() {
        lr = GetComponent<LineRenderer>();
    }

    void Update() {
       
        if (Input.GetMouseButtonDown(0)) {
            StartGrapple();
        }
        else if (Input.GetMouseButtonUp(0)) {
            StopGrapple();
        }
    }

    //Called after Update
    void LateUpdate() {
         if (isGrappling) {
            UpdateGrapple();
            CheckGrappleEnd();
        }
        //DrawRope();
    }
    void enemyGrapple() {
        if (playerMovement != null && playerMovement.grounded) { // Check if grounded
        isGrappling = false;
            return;
        }
    RaycastHit hit;

    if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, whatIsRed)) {
        grapplePoint = hit.point;
        isGrappling = true;

        // Disable any existing SpringJoint
        if (player.GetComponent<SpringJoint>()) {
            Destroy(player.GetComponent<SpringJoint>());
        }
        
        // Apply force to move the player toward the grapple point
        Vector3 direction = (grapplePoint - player.position).normalized;
        float grappleSpeed = 75f; // Adjust speed as needed
        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (rb != null) {
            rb.linearVelocity = direction * grappleSpeed; // Fix: use velocity instead of linearVelocity
        }

        // Initialize Line Renderer
        lr.positionCount = 2;
        currentGrapplePosition = gunTip.position;
    }
}


    void groundGrapple(){
    RaycastHit hit;
    if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, whatIsGrappleable)) {
        
        grapplePoint = hit.point;
            isGrappling = true;

            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);
            joint.maxDistance = distanceFromPoint * 0.3f;
            joint.minDistance = distanceFromPoint * 0.1f;

            joint.spring = 4f;
            joint.damper = 4.5f;
            joint.massScale = 4.5f;

            // Initialize Line Renderer
            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;
    }
    }
    /// <summary>
    /// Call whenever we want to start a grapple
    /// </summary>
     void StartGrapple() {
        RaycastHit hit;
        // If grappling to a Red Boi (enemy)
        if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, whatIsRed)) {
         enemyGrapple();
        }
        // If grappling to the ground
        else if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, whatIsGrappleable)) {
         groundGrapple();
        }
    }     void UpdateGrapple() {
        if (!isGrappling) return;

        // Update line renderer for Red Boi grapple
        if (lr.positionCount > 0) {
            currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);
            lr.SetPosition(0, gunTip.position);
            lr.SetPosition(1, currentGrapplePosition);
        }

        // Stop grappling if the player reaches the grapple point
        if (Vector3.Distance(player.position, grapplePoint) < 2f) {
            StopGrapple();
        }
    }
    void CheckGrappleEnd() {
    float distance = Vector3.Distance(player.position, grapplePoint);

    if (distance < 2f) { // Threshold for reaching the Red Boi
        LaunchPlayer();
    }
}

void LaunchPlayer() {
    lr.positionCount = 0; // Remove grapple line

    Rigidbody rb = player.GetComponent<Rigidbody>();
    if (rb != null) {
        Vector3 launchForce = new Vector3(5f, 20f, 0); // Adjust upward force as needed
        rb.linearVelocity = launchForce;
    }
}

    /// <summary>
    /// Call whenever we want to stop a grapple
    /// </summary>
   void StopGrapple() {
        isGrappling = false;
        lr.positionCount = 0;

        if (joint != null) {
            Destroy(joint);
        }
   }

    private Vector3 currentGrapplePosition;
    
    void DrawRope() {
        //If not grappling, don't draw rope
        if (!joint) return;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 2f);
        
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);
    }

    public bool IsGrappling() {
        return joint != null;
    }

    public Vector3 GetGrapplePoint() {
        return grapplePoint;
    }
}
