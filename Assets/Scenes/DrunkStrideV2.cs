using UnityEngine;

public class DrunkStrideV2 : MonoBehaviour
{
    //Time interval for the change of trajectory of the Agent
    [Range(1,10)] public float timeInterval = 5f;

    private float randomTime;

    //Speed of the Agent
    [Range(1,100)]public float velocità_tangenziale= 1;

    private float angle= 0;

    //We take account of the sense of rotation
    private bool counterClockWise = true;

    private Vector3 centroCerchio;

    private float maxRadius=0;

    private float generated_radius=0;

    //******************************
    private GameObject terrain;
    private Bounds limitiPiattaforma;
    private Vector3 plane_size;
    //*********************************

    //Debug variable
    public bool DrawCircle=true;
    public bool drawRay = true;

    // Start is called before the first frame update
    void Start()
    {
        terrain= GameObject.FindWithTag("Piattaforma");
        plane_size = terrain.transform.localScale * 10;

        limitiPiattaforma= new Bounds(terrain.transform.position, plane_size);

        CalculateVariousInfo();
    }

    void FixedUpdate()
    {
        Vector3 positionOffset= ComputePositionOffset(angle);
        transform.position = new Vector3(centroCerchio.x, transform.position.y, centroCerchio.z) + positionOffset;

        float velocità_angolare=0f;
        velocità_angolare = velocità_tangenziale / generated_radius;

        if(counterClockWise)
        {
            transform.LookAt(centroCerchio);
            transform.Rotate(0,90,0);

            angle += Time.deltaTime * velocità_angolare;

            //angle += Time.deltaTime * velocità_tangenziale;
        }
        else{
            transform.LookAt(centroCerchio);
            transform.Rotate(0,-90,0);

            angle -= Time.deltaTime * velocità_angolare;

            //angle -= Time.deltaTime * velocità_tangenziale;
        }
    }

    //Given an angle we calculate the position on a circle centered in (0,0), and radius generated_radius
    private Vector3 ComputePositionOffset( float a)
    {
        // Compute the position of the object
        Vector3 positionOffset = new Vector3(
            Mathf.Cos( a ) * generated_radius,
            0,
            Mathf.Sin( a ) * generated_radius
        );

        return positionOffset;
    }


    /*
    Function that every time interval, calculate the maximum radius and make sure that 
    it doesn't fall off the platform.
    */
    public void CalculateVariousInfo()
    {
        float raggio_precedente= generated_radius;
        RaycastHit hit;

        if(!counterClockWise){
            if(Physics.Raycast(transform.position, -transform.right, out hit, Mathf.Infinity)){
                maxRadius = (float)Mathf.Ceil(hit.distance/2);
            }
        }
        else{
            if(Physics.Raycast(transform.position, transform.right, out hit, Mathf.Infinity)){
                maxRadius = (float)Mathf.Ceil(hit.distance/2);
            }
        }

        if(drawRay) Debug.DrawRay(transform.position, hit.point, Color.black,2);

        while(true){
            generateCircle(maxRadius);
            if(!checkCircle(maxRadius, centroCerchio)){
                maxRadius-=1;
            }
            else{
                break;
            }
        }

        if(maxRadius <1f) maxRadius=1f;

        while(true){
            generated_radius = Random.Range(1f,maxRadius);
            generateCircle(generated_radius);
            if(checkCircle(generated_radius, centroCerchio)){
                break;
            }
            else{
                maxRadius = raggio_precedente;
                generateCircle(generated_radius);
                if(counterClockWise)
                {
                    counterClockWise=false;
                    angle-=180 * Mathf.Deg2Rad;
                } 
                else{
                    counterClockWise=true;
                    angle+=180 * Mathf.Deg2Rad;
                }
                Debug.Log("No nuova circ, continuo sulla precedente");
             }   
        }

        //Change of rotation
        if(counterClockWise)
        {
            counterClockWise=false;
            angle-=180 * Mathf.Deg2Rad;
        } 
        else{
            counterClockWise=true;
            angle+=180 * Mathf.Deg2Rad;
        } 

        //Generation of the new time interval
        randomTime= Random.Range(0.1f,timeInterval);
        Invoke(nameof(CalculateVariousInfo), randomTime);
    }

    //Check if the circle defined by the argument of this method is completely inside
    //the platform or not
    private bool checkCircle(float radius, Vector3 cetroCerchio)
    {
        for(int i=0; i<=360; i+=10)
        {
            float angle = i;
            angle*=Mathf.Deg2Rad;
             // Compute the position of the object
            Vector3 positionOffset = new Vector3(
                Mathf.Cos( angle ) * radius,
                0,
                Mathf.Sin( angle ) * radius);

            Vector3 posOnCircle = centroCerchio + positionOffset;

            if(!limitiPiattaforma.Contains(posOnCircle)){
                return false;
            }
        }
        return true;
    }


    //Generate the center of the circle accoordin to the sense of the rotation.
    private void generateCircle(float radius)
    {
        if(counterClockWise) centroCerchio= transform.position + transform.right*radius;
        else centroCerchio = transform.position - transform.right*radius;
    }


    private void OnDrawGizmos() {
        // UnityEditor.Handles.Label(transform.position + 2f * transform.up, "Dimensione piano: "+plane_size.x+"x"+plane_size.z+
        //                                                                   "\n"+"Posizione Relativa: "+transform.position+
        //                                                                   "\n"+"Raggio massimo: "+maxRadius);

        UnityEditor.Handles.Label(transform.position + 2f*transform.up, "Raggio massimo: "+maxRadius+"\n"+
                                                                        "Raggio generato: "+generated_radius+"\n"+
                                                                        //"Angolo: "+angle * Mathf.Rad2Deg+"\n"+
                                                                        "Intervallo di tempo: "+randomTime);

        if(DrawCircle){
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawWireArc(new Vector3(centroCerchio.x, 0, centroCerchio.z), 
                                            Vector3.up, -transform.right, 
                                            360,
                                            generated_radius, 2);
        }
    }
}
