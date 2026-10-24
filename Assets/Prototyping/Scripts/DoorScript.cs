using UnityEngine;

public class DoorScript : Enemy
{
    public GameObject Door;


    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (dState == DetectionState.confused)
        {
            //Debug.Log(dState);
            Door.gameObject.SetActive(false);
        } else
        {
            //Debug.Log(dState);
            Door.gameObject.SetActive(true);
        }
    }

    
        
    
}
